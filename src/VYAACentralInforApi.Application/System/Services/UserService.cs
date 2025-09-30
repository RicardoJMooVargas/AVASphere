using VYAACentralInforApi.Application.System.Interfaces;
using VYAACentralInforApi.Domain.System;
using VYAACentralInforApi.Domain.System.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace VYAACentralInforApi.Application.System.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<Users?> GetUserByIdAsync(string id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<Users?> GetUserByUserNameAsync(string userName)
        {
            return await _userRepository.GetUserByUserNameAsync(userName);
        }

        public async Task<Users> CreateUserAsync(Users user)
        {
            // Hash the password before saving
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.HashPassword = HashPassword(user.Password);
                user.Password = string.Empty; // Clear plain text password
            }

            user.CreateAt = DateTime.UtcNow;
            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<Users> UpdateUserAsync(Users user)
        {
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _userRepository.UserExistsAsync(userName);
        }

        public async Task InitializeDefaultUserAsync()
        {
            const string defaultUserName = "admin";
            
            var userExists = await _userRepository.UserExistsAsync(defaultUserName);
            
            if (!userExists)
            {
                var defaultUser = new Users
                {
                    UserName = defaultUserName,
                    Name = "Administrator",
                    LastName = "System",
                    Rol = "Admin",
                    Password = "admin123",
                    Status = true,
                    Verified = true,
                    CreateAt = DateTime.UtcNow
                };

                await CreateUserAsync(defaultUser);
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}