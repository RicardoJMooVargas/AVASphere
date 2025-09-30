using VYAACentralInforApi.Domain.System;

namespace VYAACentralInforApi.Application.System.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users?> GetUserByIdAsync(string id);
        Task<Users?> GetUserByUserNameAsync(string userName);
        Task<Users> CreateUserAsync(Users user);
        Task<Users> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UserExistsAsync(string userName);
        Task InitializeDefaultUserAsync();
    }
}