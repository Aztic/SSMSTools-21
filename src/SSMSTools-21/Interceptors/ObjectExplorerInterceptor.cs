using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSMSTools_21.Commands.MultiDbQueryRunner;
using SSMSTools_21.Interceptors.Interfaces;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSMSTools_21.Interceptors
{
    public class ObjectExplorerInterceptor : IObjectExplorerInterceptor
    {
        private readonly IObjectExplorerService _objectExplorerService;
        private readonly IServiceProvider _serviceProvider;
        private TreeView _treeView;

        public ObjectExplorerInterceptor(IObjectExplorerService objectExplorerService, IServiceProvider serviceProvider)
        {
            _objectExplorerService = objectExplorerService;
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_objectExplorerService == null)
            {
                throw new ArgumentNullException(nameof(IObjectExplorerService));
            }
                

            // Use reflection to get the 'tree' property
            var treeProperty = _objectExplorerService.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(prop => string.Equals(prop.Name, "tree", StringComparison.OrdinalIgnoreCase));

            _treeView = treeProperty?.GetValue(_objectExplorerService, null) as TreeView;

            if (_treeView != null)
            {
                _treeView.ContextMenuStripChanged += TreeView_ContextMenuStripChanged;
            }
        }

        private void TreeView_ContextMenuStripChanged(object sender, EventArgs e)
        {
            if (_treeView?.ContextMenuStrip?.Items == null)
            {
                return;
            }

#if DEBUG
            var objectExplorerMenuItemName = "SSMSTools_21 DEBUG";
#else
            var objectExplorerMenuItemName = "SSMSTools_21";
#endif

            if (_treeView.ContextMenuStrip.Items.Cast<ToolStripItem>().Any(x => x.Text == objectExplorerMenuItemName))
            {
                return;
            }

            ToolStripMenuItem packageContextMenu = new ToolStripMenuItem(objectExplorerMenuItemName);

            // TODO: Inject this dynamically
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
            ThreadHelper.ThrowIfNotOnUIThread();
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
            ThreadHelper.ThrowIfNotOnUIThread();
            var shell = _serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell5;
            if (shell == null)
            {
                return Color.Gray;
            }

            return Color.FromArgb((int)shell.GetThemedColorRgba(themeKey));
        }
    }
}
