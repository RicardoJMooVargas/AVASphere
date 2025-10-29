using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AVASphere.ApplicationCore.Common.DTOs;
using Microsoft.IdentityModel.Tokens;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.System.Interfaces;
using AVASphere.Infrastructure.System.Configuration;

namespace AVASphere.Infrastructure.System.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    // ✅ Sobrecarga para UserResponse
    public string GenerateToken(UserResponse user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUsers.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("Name", user.Name ?? string.Empty),
            new Claim("LastName", user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.RolName ?? "User"),
            new Claim("Status", user.Status ?? "Unknown")
        };

        return CreateToken(claims);
    }

    // ✅ Sobrecarga para User (entidad)
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Name", user.Name ?? string.Empty),
            new Claim("LastName", user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Rol?.Name ?? "User"),
            new Claim("Status", user.Status ?? "Unknown")
        };

        return CreateToken(claims);
    }

    // ✅ Método privado para crear el token
    private string CreateToken(List<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}