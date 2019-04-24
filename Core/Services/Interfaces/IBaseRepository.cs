using Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface IBaseRepository<T> where T : EntityBase, new ()
    {
        Task<IList<T>> GetAll();
        Task<T> Get(string id);
        Task Add(T entity);
        Task Delete(string id);
        Task Upsert(T entity);
    }
}