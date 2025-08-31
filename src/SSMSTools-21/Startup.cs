using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using SSMSTools_21.Factories;
using SSMSTools_21.Factories.Interfaces;
using SSMSTools_21.Managers;
using SSMSTools_21.Managers.Interfaces;
using SSMSTools_21.Services;
using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using SSMSTools_21.Services.Interfaces;
using SSMSTools_21.Windows.DatabaseGroupManager;
using SSMSTools_21.Windows.Interfaces;
using SSMSTools_21.Windows.MultiDbQueryRunner;
using ConfigurationManager = SSMSTools_21.Managers.ConfigurationManager;

namespace SSMSTools_21
{
    internal class Startup
    {
        private readonly AsyncPackage _package;

        public Startup(AsyncPackage package)
        {
            _package = package;
        }

        public IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            RegisterManagers(serviceCollection);
            RegisterServices(serviceCollection);
            RegisterWindows(serviceCollection);
            RegisterFactories(serviceCollection);

            // Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private void RegisterManagers(IServiceCollection services)
        {
            services.AddTransient<IMessageManager, MessageManager>();
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        }

        private void RegisterServices(IServiceCollection services)
        {
            // Register DTE2
            services.AddTransient(provider =>
            {
                return _package.GetServiceAsync(typeof(DTE)).Result as DTE2;
            });

            // Register IObjectExplorerService
            services.AddTransient(provider =>
            {
                return _package.GetServiceAsync(typeof(IObjectExplorerService)).Result as IObjectExplorerService;
            });

            services.AddSingleton<IUIService, UiService>();
        }

        private void RegisterWindows(IServiceCollection services)
        {
            services.AddTransient<IMultiDbQueryRunnerWindow, MultiDbQueryRunnerWindow>();
            services.AddTransient<IDatabaseGroupManagerWindow, DatabaseGroupManagerWindow>();
        }

        private void RegisterFactories(IServiceCollection services)
        {
            services.AddTransient<IWindowFactory, WindowFactory>();
        }
    }
}