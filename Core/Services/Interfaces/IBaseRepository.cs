using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface IBaseRepository<T> where T : EntityBase, new ()
    {
        Task<IList<T>> GetAll();
        Task<T> Get(string id);
        Task<string> Add(T entity);
        Task Delete(string id);
        Task Upsert(T entity);
        Task<IList<T>> GetWhere(Expression<Func<T, bool>> predicate);
    }
}