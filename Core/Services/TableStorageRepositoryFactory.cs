using Core.Services.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace Core.Services
{
    public class TableStorageRepositoryFactory<T> : ITableRepositoryFactory<T> where T : TableEntity, new()
    {
        public ITableRepository<T> GetInstance(string connectionString, string tableName)
        {
            return new TableStorageRepository<T>(connectionString, tableName);
        }
    }
}