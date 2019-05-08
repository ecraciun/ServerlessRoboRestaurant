﻿using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

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
                entity.Id = Guid.NewGuid().ToString();
            }

            var result = await _documentClient.CreateDocumentAsync(_collectionUri, entity, disableAutomaticIdGeneration: true);
            return result.Resource.Id;

        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            try
            {
                await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Constants.DatabaseName, _collectionId, id));
                return true;
            }
            catch (DocumentClientException ex) when (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                return true; // well it's not there anyway
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
                result = await GetAllResultsFromDocumentQuery(queryable);
            }

            return result;
        }

        public async Task<IList<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            List<T> result;

            using (var queryable = _documentClient.CreateDocumentQuery<T>(
                _collectionUri, new FeedOptions { MaxItemCount = 100 })
                .Where(predicate)
                .AsDocumentQuery())
            {
                result = await GetAllResultsFromDocumentQuery(queryable);
            }

            return result;
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
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> TryUpdateWithRetry(T entity, Action<T> entityUpdateAction, int retryCount = Constants.DefaultTryUpdateRetryCount)
        {
            if (entity == null) throw new ArgumentException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id)) throw new ArgumentException(nameof(entity.Id));
            if (string.IsNullOrEmpty(entity.ETag)) throw new ArgumentException(nameof(entity.ETag));
            if (entityUpdateAction == null) throw new ArgumentException(nameof(entityUpdateAction));
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

        private async Task<List<T>> GetAllResultsFromDocumentQuery(IDocumentQuery<T> queryable)
        {
            var result = new List<T>();
            while (queryable.HasMoreResults)
            {
                foreach (var entity in await queryable.ExecuteNextAsync<T>())
                {
                    result.Add(entity);
                }
            }
            return result;
        }
    }
}