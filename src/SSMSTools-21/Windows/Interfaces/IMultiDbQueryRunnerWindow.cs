using SSMSTools_21.Models;

namespace SSMSTools_21.Windows.Interfaces
{
    internal interface IMultiDbQueryRunnerWindow : IBaseWindow
    {
        void SetServerInformation(ConnectedServerInformation serverInformation);
    }
}