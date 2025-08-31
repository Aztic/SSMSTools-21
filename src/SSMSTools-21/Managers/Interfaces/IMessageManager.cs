using System;

namespace SSMSTools_21.Managers.Interfaces
{
    public interface IMessageManager
    {
        void ShowMessageBox(IServiceProvider serviceProvider, string title, string message);

        void ShowSimpleMessageBox(string content);
    }
}