using Core.Entities;

namespace Core.Services.Interfaces
{
    public interface IBaseRepositoryFactory<T> where T: EntityBase, new()
    {
        IBaseRepository<T> GetInstance(string endpointUri, string key, string collectionId);
    }
}