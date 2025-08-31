using SSMSTools_21.Configurations.SavedDatabaseGroups;
using SSMSTools_21.Models;
using System.Collections.Generic;

namespace SSMSTools_21.Mappers
{
    public static class DatabaseGroupMapper
    {
        public static DatabaseGroup MapToModel(this SavedDatabaseGroup databaseGroup)
        {
            return new DatabaseGroup
            {
                Id = databaseGroup.Id,
                Title = databaseGroup.Title,
                Databases = new List<string>(databaseGroup.Databases)
            };
        }

        public static SavedDatabaseGroup MapToSavedDatabaseGroup(this DatabaseGroup databaseGroup)
        {
            return new SavedDatabaseGroup
            {
                Id = databaseGroup.Id,
                Title = databaseGroup.Title,
                Databases = new List<string>(databaseGroup.Databases)
            };
        }
    }
}