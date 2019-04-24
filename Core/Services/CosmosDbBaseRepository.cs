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

namespace Core.Services
{
    public class CosmosDbBaseRepository<T> : IBaseRepository<T> where T : EntityBase, new()
    {
        private readonly DocumentClient _documentClient;
        private readonly string _collectionId;
        private readonly Uri _collectionUri;

        public CosmosDbBaseRepository(DocumentClient documentClient, string collectionId)
        {
            _documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _collectionId = string.IsNullOrEmpty(collectionId) ? throw new ArgumentNullException(nameof(collectionId)) : collectionId;
            _collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, _collectionId);
        }

        public async Task Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString(); // or maybe throw an exception
            }

            await _documentClient.CreateDocumentAsync(_collectionUri, entity, disableAutomaticIdGeneration: true);
            
        }

        public async Task Delete(string id)
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

        public async Task<T> Get(string id)
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

        public async Task<IList<T>> GetAll()
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

        public async Task<IList<T>> GetWhere(Expression<Func<T, bool>> predicate)
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

        public async Task Upsert(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id)) throw new ArgumentNullException(nameof(entity.Id));

            await _documentClient.UpsertDocumentAsync(_collectionUri, entity);
        }
    }
}