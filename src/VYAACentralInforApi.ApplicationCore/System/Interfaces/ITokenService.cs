namespace VYAACentralInforApi.ApplicationCore.System.Interfaces;

public interface ITokenService
{
    string GenerateToken(Users user);
}