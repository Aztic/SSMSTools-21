using EnvDTE;
using EnvDTE80;
using SSMSTools_21.Interceptors.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMSTools_21.Interceptors
{
    public class EventInterceptor : IEventInterceptor
    {
        private readonly DTE2 _dte;

        public EventInterceptor(DTE2 dte)
        {
            _dte = dte;
        }

        public void Initialize()
        {
            var allEvents = _dte.Events.get_CommandEvents(null, 0);
            allEvents.BeforeExecute += (string guid, int id, object inObj, object outObj, ref bool cancel) =>
            {
                System.Diagnostics.Debug.WriteLine($"Command Fired: GUID={guid} ID={id}");
            };
        }
    }
}
