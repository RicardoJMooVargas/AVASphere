using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.System.Interfaces;

public interface ITokenService
{
    string GenerateToken(UserResponse user);
    //string GenerateToken(User user);
}