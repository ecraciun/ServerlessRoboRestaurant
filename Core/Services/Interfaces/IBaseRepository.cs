using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface IBaseRepository<T> where T : EntityBase, new()
    {
        /// <summary>
        ///     Gets all entities asynchronously
        /// </summary>
        /// <returns>An awaitable Task that will complete with the list of all entities</returns>
        Task<IList<T>> GetAllAsync();

        /// <summary>
        ///     Get an entity by its id asynchronously
        /// </summary>
        /// <param name="id">The entity's id</param>
        /// <returns>An awaitable Task that will complete with the found entity</returns>
        Task<T> GetAsync(string id);

        /// <summary>
        ///     Inserts a new entity asynchronously
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>An awaitable Task that will complete with the id of the new entity</returns>
        Task<string> AddAsync(T entity);

        /// <summary>
        ///     Removes an entity by its id asynchronously
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>An awaitable Task that will complete with true if the delete succedeed or false otherwise</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        ///     Gets a list of filtered entities asynchronously
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <returns>An awaitable Task that will complete with the list of entities satisfying the expression</returns>
        Task<IList<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     Updates an entity asynchronously
        /// </summary>
        /// <param name="entity">The updated entity</param>
        /// <returns>An awaitable Task that will complete with true if the update succedeed or false otherwise</returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        ///     Tries to update an entity asynchronously, retrying a number of times
        /// </summary>
        /// <param name="entity">The updated entity</param>
        /// <param name="entityUpdateAction">Action that re-applies updates to the entity in case a retry is needed</param>
        /// <param name="retryCount">The number of times the update should retry when there are concurrency issues</param>
        /// <returns>An awaitable Task that will complete with true if the update succedeed or false otherwise</returns>
        Task<bool> TryUpdateWithRetry(T entity, Action<T> entityUpdateAction, int retryCount = Constants.DefaultTryUpdateRetryCount);
    }
}