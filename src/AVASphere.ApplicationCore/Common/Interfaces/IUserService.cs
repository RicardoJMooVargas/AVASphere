using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IUserService
{
    Task<UserResponse> SearchUsersAsync(int idUsers);
    Task<UserResponse> NewUsersAsync(UserCreateRequest user);
    Task<UserResponse> EditUsersAsync(UserUpdateRequest user);
}