using Microsoft.VisualStudio.Shell;
using SSMSTools_21.Services.Interfaces;

namespace SSMSTools_21.Services
{
    internal class UIService : IUIService
    {
        public void ValidateUIThread()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
        }
    }
}