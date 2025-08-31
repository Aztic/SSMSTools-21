using SSMSTools_21.Models;
using System;
using System.Collections.Generic;

namespace SSMSTools_21.Windows.Interfaces
{
    internal interface IDatabaseGroupManagerWindow : IBaseWindow
    {
        event EventHandler ContentSaved;

        event EventHandler RefreshDatabaseList;

        void SetDatabaseGroupId(Guid? id);

        void SetDatabaseGroupName(string databaseGroupName);

        void SetDatabases(IEnumerable<CheckboxItem> databases, bool resetSelectedDatabases);
    }
}