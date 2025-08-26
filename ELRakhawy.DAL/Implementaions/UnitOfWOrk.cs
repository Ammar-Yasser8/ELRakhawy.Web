using Elrakhawy.DAL.Data;
using ELRakhawy.EL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.DAL.Implementaions
{
    public class UnitOfWOrk : IUnitOfWork
    {
        private readonly AppDBContext _context;
        private readonly Dictionary<Type, object> _repositories;
        public UnitOfWOrk(AppDBContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenaricRepository<T>(_context);
            }
            return (IGenericRepository<T>)_repositories[type];

        }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
