using System.Collections.Generic;
using System.Linq;

namespace SSMSTools_21.Configurations.SavedDatabaseGroups
{
    public class SavedDatabaseGroupsConfiguration
    {
        public IEnumerable<SavedDatabaseGroup> Global { get; set; } = Enumerable.Empty<SavedDatabaseGroup>();
    }
}