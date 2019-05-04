using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface IBaseRepository<T> where T : EntityBase, new ()
    {
        Task<IList<T>> GetAllAsync();
        Task<T> GetAsync(string id);
        Task<string> AddAsync(T entity);
        Task DeleteAsync(string id);
        Task UpsertAsync(T entity);
        Task<IList<T>> Async(Expression<Func<T, bool>> predicate);
        Task<bool> UpdateAsync(T entity);
        Task<bool> TryUpdateWithRetry(T entity, Action<T> entityUpdateAction, int retryCount = Constants.DefaultTryUpdateRetryCount);
    }
}