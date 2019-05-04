using Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Core.Entities;
using Microsoft.Azure.Documents.Linq;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.Azure.Documents;
using System.Net;

namespace Core.Services
{
    public class CosmosDbBaseRepository<T> : IBaseRepository<T> where T : EntityBase, new()
    {
        private readonly IDocumentClient _documentClient;
        private readonly string _collectionId;
        private readonly Uri _collectionUri;

        public CosmosDbBaseRepository(IDocumentClient documentClient, string collectionId)
        {
            _documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _collectionId = string.IsNullOrEmpty(collectionId) ? throw new ArgumentNullException(nameof(collectionId)) : collectionId;
            _collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, _collectionId);
        }

        public async Task<string> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString(); // or maybe throw an exception
            }

            var result = await _documentClient.CreateDocumentAsync(_collectionUri, entity, disableAutomaticIdGeneration: true);
            return result.Resource.Id;
            
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            try
            {
                await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Constants.DatabaseName, _collectionId, id));
            }
            catch(DocumentClientException ex) when (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                //TODO: treat ex
            }
            catch(Exception ex)
            {
                //TODO: treat ex
            }
        }

        public async Task<T> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            try
            {
                var response = await _documentClient.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(Constants.DatabaseName, _collectionId, id));
                return response.Document;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IList<T>> GetAllAsync()
        {
            List<T> result = new List<T>();

            using (var queryable = _documentClient.CreateDocumentQuery<T>(
                _collectionUri, new FeedOptions { MaxItemCount = 100 })
                .AsDocumentQuery())
            {
                while (queryable.HasMoreResults)
                {
                    foreach(var entity in await queryable.ExecuteNextAsync<T>())
                    {
                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public async Task<IList<T>> Async(Expression<Func<T, bool>> predicate)
        {
            List<T> result = new List<T>();

            using (var queryable = _documentClient.CreateDocumentQuery<T>(
                _collectionUri, new FeedOptions { MaxItemCount = 100 })
                .Where(predicate)
                .AsDocumentQuery())
            {
                while (queryable.HasMoreResults)
                {
                    foreach (var entity in await queryable.ExecuteNextAsync<T>())
                    {
                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public async Task UpsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id)) throw new ArgumentNullException(nameof(entity.Id));

            await _documentClient.UpsertDocumentAsync(_collectionUri, entity);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id)) throw new ArgumentException(nameof(entity.Id));
            if (string.IsNullOrEmpty(entity.ETag)) throw new ArgumentException(nameof(entity.ETag));

            var accessCondition = new AccessCondition()
            {
                Condition = entity.ETag,
                Type = AccessConditionType.IfMatch
            };

            try
            {
                await _documentClient.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(Constants.DatabaseName, _collectionId, entity.Id),
                    entity, new RequestOptions
                    {
                        AccessCondition = accessCondition
                    });
            }
            catch(DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> TryUpdateWithRetry(T entity, Action<T> entityUpdateAction, int retryCount = Constants.DefaultTryUpdateRetryCount)
        {
            if (entity == null) throw new ArgumentException(nameof(entity));
            if(entityUpdateAction == null) throw new ArgumentException(nameof(entityUpdateAction));
            int retries = 0;

            entityUpdateAction(entity);
            bool updateResult = await UpdateAsync(entity); ;

            while (retries < retryCount && updateResult == false) 
            {
                retries++;
                entity = await GetAsync(entity.Id);
                entityUpdateAction(entity);
                updateResult = await UpdateAsync(entity);
            }

            return updateResult;
        }
    }
}