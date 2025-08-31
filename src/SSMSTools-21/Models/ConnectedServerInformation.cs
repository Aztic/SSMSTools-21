using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SSMSTools_21.Models
{
    public class ConnectedServerInformation
    {
        public string ServerName { get; set; }
        public ICollection<CheckboxItem> Databases { get; set; } = new Collection<CheckboxItem>();
    }
}