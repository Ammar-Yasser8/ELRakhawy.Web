using Elrakhawy.DAL.Data;
using ELRakhawy.DAL.Security;
using ELRakhawy.EL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.DAL.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(AppDBContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                var users = new List<AppUser>
                {
                    new AppUser
                    {
                        FirstName = "Super",
                        LastName = "Admin",
                        Email = "superadmin@tax.com",
                        PasswordHash = PasswordHasher.HashPassword("892001ammar"),
                        Role = UserRole.SuperAdmin
                    },
                    new AppUser
                    {
                        FirstName = "Viewer",
                        LastName = "User",
                        Email = "viewer@tax.com",
                        PasswordHash = PasswordHasher.HashPassword("892001ammar"),
                        Role = UserRole.Viewer
                    },
                    new AppUser
                    {
                        FirstName = "Editor",
                        LastName = "User",
                        Email = "editor@tax.com",
                        PasswordHash = PasswordHasher.HashPassword("892001ammar"),
                        Role = UserRole.Editor
                    },
                    new AppUser
                    {
                        FirstName = "Added",
                        LastName = "User",
                        Email = "added@tax.com",
                        PasswordHash = PasswordHasher.HashPassword("892001ammar"),
                        Role = UserRole.Added
                    },
                    new AppUser
                    {
                        FirstName = "Clear",
                        LastName = "User",
                        Email = "clear@tax.com",
                        PasswordHash = PasswordHasher.HashPassword("892001ammar"),
                        Role = UserRole.Clear
                    }
                };

                context.Users.AddRange(users);
                await context.SaveChangesAsync();
            }
        }
    }
}
