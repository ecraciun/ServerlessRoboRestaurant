using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.Documents.Client;
using System;

namespace Core.Services
{
    public class CosmosDbBaseRepositoryFactory<T> : IBaseRepositoryFactory<T> where T : EntityBase, new()
    {
        private static DocumentClient _documentClient;
        private CosmosDbBaseRepository<T> _repositoryInstance;

        public IBaseRepository<T> GetInstance(string endpointUri, string key, string collectionId)
        {
            if (string.IsNullOrEmpty(endpointUri)) throw new ArgumentNullException(nameof(endpointUri));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(collectionId)) throw new ArgumentNullException(nameof(collectionId));

            if (_documentClient == null)
            {
                _documentClient = new DocumentClient(new Uri(endpointUri), key);
            }
            if (_repositoryInstance == null)
            {
                _repositoryInstance = new CosmosDbBaseRepository<T>(_documentClient, collectionId);
            }
            return _repositoryInstance;
        }
    }
}