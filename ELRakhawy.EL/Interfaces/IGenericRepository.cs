using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        ICollection<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? includeEntities = null);
        T? GetOne(Expression<Func<T, bool>>? predicate = null, string? includeEntities = null);
        void Add(T entity);
        void AddRange(IEnumerable<T> Entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> Entities);
    }
}
