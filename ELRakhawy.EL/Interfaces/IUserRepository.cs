using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByIdAsync(int id);
        Task AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task<IEnumerable<AppUser>> GetAllAsync();
    }
}
