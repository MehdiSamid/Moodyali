using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Moodyali.Shared.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    /// <summary>
    /// Hashes a password using PBKDF2.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password string, including salt and iteration count.</returns>
    public static string HashPassword(string password)
    {
        // Generate a 16-byte salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt and hash, then convert to base64 string
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="storedHash">The stored hash (including salt).</param>
    /// <returns>True if the password is correct, false otherwise.</returns>
    public static bool VerifyPassword(string password, string storedHash)
    {
        // Convert the stored hash from base64 string to byte array
        byte[] hashBytes = Convert.FromBase64String(storedHash);

        // Extract the salt
        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

        // Extract the stored hash
        byte[] storedPasswordHash = new byte[HashSize];
        Buffer.BlockCopy(hashBytes, SaltSize, storedPasswordHash, 0, HashSize);

        // Hash the provided password with the extracted salt
        byte[] computedHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Compare the computed hash with the stored hash
        return computedHash.SequenceEqual(storedPasswordHash);
    }
}
