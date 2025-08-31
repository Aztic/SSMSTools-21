using System;
using System.Collections.Generic;

namespace SSMSTools_21.Events.Arguments
{
    public class DatabaseGroupSavedEventArgs : EventArgs
    {
        public Guid? DatabaseGroupId { get; set; }
        public string DatabaseGroupName { get; set; }
        public IEnumerable<string> Databases { get; set; }
    }
}