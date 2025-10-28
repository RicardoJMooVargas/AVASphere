using System.Security.Cryptography;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AVASphere.Infrastructure.Common.Security;


public class EncryptionService : IEncryptionService
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        // Generar un salt aleatorio
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Derivar la clave
        byte[] hashed = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);

        // Combinar salt y hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hashed, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            // Extraer el salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            // Calcular el hash de la contraseña proporcionada
            byte[] expectedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 20);

            // Comparar los hashes
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != expectedHash[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}