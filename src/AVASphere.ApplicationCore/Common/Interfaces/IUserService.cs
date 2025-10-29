using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IUserService
{
    Task<UserResponse> SearchUsersAsync(int? idUsers, string? userName);
    Task<UserResponse> NewUsersAsync(UserCreateRequest user);
    Task<UserResponse> EditUsersAsync(UserUpdateRequest user);
    Task<LoginResponse> AuthenticateUserAsync(LoginRequest loginRequest);
}