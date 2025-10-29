using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.ApplicationCore.System.Interfaces;

public interface ITokenService
{
    string GenerateToken(UserResponse user);
    string GenerateToken(User user);
}