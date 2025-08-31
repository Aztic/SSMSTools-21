using SSMSTools_21.Factories.Interfaces;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace SSMSTools_21.Factories
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateWindow<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}