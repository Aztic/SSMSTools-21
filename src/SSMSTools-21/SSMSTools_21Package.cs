using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSMSTools_21.Commands.MultiDbQueryRunner;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SSMSTools_21
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SSMSTools_21Package.PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public class SSMSTools_21Package : AsyncPackage
    {
        /// <summary>
        /// SSMSTools_21Package GUID string.
        /// </summary>
        public const string PackageGuidString = "2be4a5ba-a298-44aa-9ca9-991006011e06";
        private System.IServiceProvider _serviceProvider;
        public virtual System.IServiceProvider ServiceProvider => _serviceProvider;


        private IObjectExplorerService _objectExplorerService;
        private TreeView _treeView;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var startup = new Startup(this);
            _serviceProvider = startup.ConfigureServices();
            _objectExplorerService = (IObjectExplorerService)(await GetServiceAsync(typeof(IObjectExplorerService)));

            var objectExplorerTree = _objectExplorerService
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(prop => string.Equals(prop.Name, "tree", StringComparison.OrdinalIgnoreCase));

            _treeView = objectExplorerTree != null ? (TreeView)objectExplorerTree.GetValue(_objectExplorerService, null) : null;

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await MultiDbQueryRunnerCommand.InitializeAsync(this);

            if (_treeView != null)
            {
                _treeView.ContextMenuStripChanged += TreeView_ContextMenuStripChanged;
            }
            
        }

        private void TreeView_ContextMenuStripChanged(object sender, EventArgs e)
        {
            if (_treeView?.ContextMenuStrip?.Items == null || _objectExplorerService == null)
            {
                return;
            }

            ToolStripMenuItem packageContextMenu = new ToolStripMenuItem(nameof(SSMSTools_21));

            var menuItems = new Collection<ToolStripMenuItem>
            {
                new ToolStripMenuItem
                {
                    Text = "Run query in multiple Databases",
                    Tag = nameof(MultiDbQueryRunnerCommand),
                    BackColor = VsColorToDrawingColor(EnvironmentColors.ToolWindowBackgroundColorKey),
                    ForeColor = VsColorToDrawingColor(EnvironmentColors.ToolWindowTextColorKey)
                }
            };

            foreach (var menuItem in menuItems)
            {
                menuItem.Click += MenuItem_Click;
                packageContextMenu.DropDownItems.Add(menuItem);
            }

            _treeView.ContextMenuStrip.Items.Add(packageContextMenu);
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                var commandName = menuItem.Tag.ToString();
                switch (commandName)
                {
                    case nameof(MultiDbQueryRunnerCommand):
                        MultiDbQueryRunnerCommand.Instance.Execute(null, null);
                        break;
                }
            }
        }

        private Color VsColorToDrawingColor(ThemeResourceKey themeKey)
        {
            if (themeKey == null)
            {
                throw new ArgumentNullException(nameof(themeKey));
            }
            
            
            if (!(GetService(typeof(SVsUIShell)) is IVsUIShell5 shell))
            {
                throw new InvalidOperationException("Failed to retrieve the shell service.");
            }
            
            return Color.FromArgb((int)shell.GetThemedColorRgba(themeKey));
        }

        #endregion
    }
}
