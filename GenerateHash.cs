using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Generador de Hash de Contraseña");
        Console.WriteLine("================================");
        Console.Write("Ingresa la contraseña: ");
        string password = Console.ReadLine() ?? "";
        
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("La contraseña no puede estar vacía");
            return;
        }

        string hash = HashPassword(password);
        Console.WriteLine($"\nHash generado:");
        Console.WriteLine(hash);
        Console.WriteLine($"\nLongitud: {hash.Length} caracteres");
        Console.WriteLine("\nQuery SQL para actualizar:");
        Console.WriteLine($"UPDATE public.\"User\" SET \"HashPassword\" = '{hash}' WHERE \"UserName\" = 'admin';");
    }

    static string HashPassword(string password)
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
}
