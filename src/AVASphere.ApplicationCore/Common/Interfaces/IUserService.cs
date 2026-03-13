using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> SearchUsersAsync(int? idUsers, string? userName);
    Task<UserResponse> NewUsersAsync(UserCreateRequest user);
    Task<UserResponse> EditUsersAsync(UserUpdateRequest user);
    Task<LoginResponse> AuthenticateUserAsync(LoginDTOs loginDtOs);
    Task<UserResponse> SetupAdminUserAsync(string userName, string password);
}