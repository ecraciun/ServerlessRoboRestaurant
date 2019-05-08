using Core.Entities;

namespace Core.Services.Interfaces
{
    public interface IBaseRepositoryFactory<T> where T: EntityBase, new()
    {
        /// <summary>
        ///     Gets the instance of the repository, supplying it the necessary details
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        /// <param name="key">Connection key</param>
        /// <param name="collectionId">Collection Id</param>
        /// <returns>The repository instance</returns>
        IBaseRepository<T> GetInstance(string endpointUri, string key, string collectionId);
    }
}