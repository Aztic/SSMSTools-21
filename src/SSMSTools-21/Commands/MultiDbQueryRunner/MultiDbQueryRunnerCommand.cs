using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using SSMSTools_21.Exceptions;
using SSMSTools_21.Factories.Interfaces;
using SSMSTools_21.Managers.Interfaces;
using SSMSTools_21.Models;
using SSMSTools_21.Services.Interfaces;
using SSMSTools_21.Windows.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SSMSTools_21.Commands.MultiDbQueryRunner
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MultiDbQueryRunnerCommand
    {
        private readonly IObjectExplorerService _objectExplorerService;
        private readonly IMessageManager _messageManager;
        private readonly IWindowFactory _windowFactory;
        private readonly IUIService _uiService;
        private readonly ISet<string> _sectionsToSanitize = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "multiple active result sets",
            "trust server certificate",
            "initial catalog"
        };

        private struct ConnectedServer
        {
            public string ServerName { get; set; }
            public string ConnectionString { get; set; }
        }
        
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1318d812-ecc9-4133-9b94-83240fcc2a34");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDbQueryRunnerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MultiDbQueryRunnerCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandId);

            _objectExplorerService = ((SSMSTools_21Package)package).ServiceProvider.GetService(typeof(IObjectExplorerService)) as IObjectExplorerService;
            _messageManager = ((SSMSTools_21Package)package).ServiceProvider.GetService(typeof(IMessageManager)) as IMessageManager;
            _windowFactory = ((SSMSTools_21Package)package).ServiceProvider.GetService(typeof(IWindowFactory)) as IWindowFactory;
            _uiService = ((SSMSTools_21Package)package).ServiceProvider.GetService(typeof(IUIService)) as IUIService;

            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MultiDbQueryRunnerCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MultiDbQueryRunnerCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MultiDbQueryRunnerCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        internal void Execute(object sender, EventArgs e)
        {
            _uiService.ValidateUIThread();
            string title = "MultiDbQueryRunner";
            var serverInformation = new ConnectedServerInformation();

            try
            {
                var connection = GetConnectedServers().Single();
                
                var connectionDatabases = GetDatabasesFromConnection(connection.ConnectionString);
                if (!connectionDatabases.Any())
                {
                    _messageManager.ShowMessageBox(this.package, title, "The connection has no available databases");
                    return;
                }

                serverInformation.ServerName = connection.ServerName;
                serverInformation.Databases = connectionDatabases.Select(x => new CheckboxItem { Name = x }).ToArray();
            }
            catch (OnlyOneObjectExplorerNodeAllowedException)
            {
                _messageManager.ShowMessageBox(this.package, title, "Only one node needs to be selected");
                return;
            }
            catch (Exception ex)
            {
                _messageManager.ShowMessageBox(this.package, title, "Unknown exception");
                return;
            }

            var window = _windowFactory.CreateWindow<IMultiDbQueryRunnerWindow>();
            window.SetServerInformation(serverInformation);
            window.Show();
        }

        /// <summary>
        /// Gets the connection string from the selected nodes
        /// </summary>
        /// <returns></returns>
        private List<ConnectedServer> GetConnectedServers()
        {
            var usedServers = new HashSet<string>();
            var connections = new List<ConnectedServer>();

            _objectExplorerService.GetSelectedNodes(out var arraySize, out var nodes);
            if (arraySize != 1)
            {
                throw new OnlyOneObjectExplorerNodeAllowedException("Only one node needs to be selected");
            }
            foreach (var node in nodes)
            {
                var connectionString = node.Connection.ConnectionString;
                var serverName = node.Connection.ServerName;
                if (usedServers.Contains(serverName))
                {
                    continue;
                }

                usedServers.Add(serverName);
                connections.Add(new ConnectedServer
                {
                    ServerName = serverName,
                    ConnectionString = connectionString
                });
            }

            return connections;
        }

        /// <summary>
        /// Given a connection string, gets the list of available databases.
        /// It creates a new connection with the server and retrieves them
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private IEnumerable<string> GetDatabasesFromConnection(string connectionString)
        {
            var databases = new Collection<string>();

            try
            {
                var cleanedConnectionString = string.Join(";", connectionString.Split(';')
                    .Select(x =>
                    {
                        var keyValue = x.Split('=');
                        var key = keyValue[0].Trim().ToLowerInvariant();
                        if (_sectionsToSanitize.Contains(key) && keyValue.Length > 1)
                        {
                            // Remove whitespace from value
                            var newKey = key.Replace(" ", string.Empty);
                            return $"{newKey}={keyValue[1]}";
                        }
                        return x;
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                string query = $"SELECT name FROM sys.databases";
                using (var conn = new SqlConnection(cleanedConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                databases.Add(reader["name"].ToString());
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _messageManager.ShowMessageBox(this.package, nameof(MultiDbQueryRunner), $"Error fetching databases for linked server: {ex.Message}");
            }

            return databases;
        }
    }
}
