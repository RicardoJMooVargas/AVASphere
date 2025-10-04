namespace VYAACentralInforApi.Application.System.Interfaces;

using VYAACentralInforApi.Domain.System;

public interface ITokenService
{
    string GenerateToken(Users user);
}