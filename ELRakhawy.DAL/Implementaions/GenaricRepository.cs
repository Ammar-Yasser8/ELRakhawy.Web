using Elrakhawy.DAL.Data;
using ELRakhawy.EL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.DAL.Implementaions
{
    public class GenaricRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDBContext _context;
        public GenaricRepository(AppDBContext context)
        {
            _context = context;
        }
        public void Add(T entity)
        {
            _context.Add(entity);
        }
        public void AddRange(IEnumerable<T> Entities)
        {
            _context.AddRange(Entities);
        }

        public ICollection<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? includeEntities = null)
        {
            var query = _context.Set<T>().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (includeEntities != null)
            {
                foreach (var includeEntity in includeEntities.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeEntity);
                }

            }
            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, bool>>? predicate = null, string? includeEntities = null)
        {
            var query = _context.Set<T>().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (includeEntities != null)
            {
                foreach (var includeEntity in includeEntities.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeEntity);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            _context.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> Entities)
        {
            _context.RemoveRange(Entities);

        }

        public void Update(T entity)
        {
            _context.Update(entity);
        }

    }
}
