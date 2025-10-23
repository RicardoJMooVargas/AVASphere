namespace AVASphere.ApplicationCore.System.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users?> GetUserByIdAsync(string id);
        Task<Users?> GetUserByUserNameAsync(string userName);
        Task<Users> CreateUserAsync(Users user);
        Task<Users> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UserExistsAsync(string userName);
    }
}