using ELRakhawy.DAL.Security;
using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.DAL.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userRepository.GetByEmailAsync(email);
            return user == null;
        }

        // Updated to match controller usage
        public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            if (!await IsEmailUniqueAsync(email))
                return false;

            var user = new AppUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<AppUser?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword, UserRole requesterRole)
        {
            if (requesterRole != UserRole.SuperAdmin)
                throw new UnauthorizedAccessException("Only SuperAdmin can reset passwords.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be null or empty.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
