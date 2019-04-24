using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface ITableRepository<T> where T : class, new ()
    {
        Task<IList<T>> GetAll();
        Task<T> Get(string id);
        Task Add(T entity);
        Task AddBulk(IList<T> entities);
        Task Delete(T entity);
        Task Upsert(T entity);
    }
}