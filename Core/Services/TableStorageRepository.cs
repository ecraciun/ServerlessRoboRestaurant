using Core.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public class TableStorageRepository<T> : ITableRepository<T> where T : TableEntity, new()
    {
        private CloudStorageAccount _cloudStorageAccount;
        private CloudTableClient _cloudTableClient;
        private CloudTable _cloudTable;

        public TableStorageRepository(string connectionString, string tableName)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            if(!CloudStorageAccount.TryParse(connectionString, out _cloudStorageAccount))
            {
                throw new ArgumentException("Connection string not valid", nameof(connectionString));
            }
            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            _cloudTable = _cloudTableClient.GetTableReference(tableName);
        }

        public async Task Add(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task AddBulk(IList<T> entities)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task<T> Get(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<T>> GetAll()
        {
            List<T> result = null;
            if(await _cloudTable.ExistsAsync())
            {
                result = new List<T>();

                TableQuery<T> query = new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, Constants.DefaultPartitionName));
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<T> resultSegment = await _cloudTable.ExecuteQuerySegmentedAsync(query, token);
                    token = resultSegment.ContinuationToken;
                    result.AddRange(resultSegment.Results);
                }
                while (token != null);
            }

            return result;
        }

        public async Task Upsert(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
