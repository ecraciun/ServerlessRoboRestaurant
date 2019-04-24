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
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.RowKey))
            {
                entity.RowKey = Guid.NewGuid().ToString(); // or maybe throw an exception
            }

            var insertOperation = TableOperation.Insert(entity);
            await _cloudTable.ExecuteAsync(insertOperation);
        }

        public async Task AddBulk(IList<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            var batchInsertOperation = new TableBatchOperation();
            foreach(var entity in entities)
            {
                if (string.IsNullOrEmpty(entity.RowKey))
                {
                    entity.RowKey = Guid.NewGuid().ToString(); 
                }
                batchInsertOperation.Insert(entity);
            }
            await _cloudTable.ExecuteBatchAsync(batchInsertOperation);
        }

        public async Task Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.RowKey)) throw new ArgumentNullException(nameof(entity.RowKey));

            var insertOperation = TableOperation.Delete(entity);
            await _cloudTable.ExecuteAsync(insertOperation);
        }

        public async Task<T> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var retriveOperation = TableOperation.Retrieve<T>(Constants.DefaultPartitionName, id);
            var result = await _cloudTable.ExecuteAsync(retriveOperation);

            return result.Result as T;
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
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.RowKey)) throw new ArgumentNullException(nameof(entity.RowKey));

            var upsertOperation = TableOperation.InsertOrReplace(entity);
            await _cloudTable.ExecuteAsync(upsertOperation);
        }
    }
}