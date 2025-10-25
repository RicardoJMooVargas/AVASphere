using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IUsersService
{
    Task<Users> SearchUsersAsync(int idUsers);
    Task<Users> NewUsersAsync(Users user);
    Task<Users> EditUsersAsync(Users user);
}