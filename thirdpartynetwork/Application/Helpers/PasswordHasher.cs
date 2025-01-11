using System.Security.Cryptography;
using thirdpartynetwork.Core.Interfaces.Helpers;

namespace thirdpartynetwork.Application.Helpers;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;      // 128 bit
    private const int KeySize = 32;       // 256 bit
    private const int Iterations = 10000; // PBKDF2 iterations
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithm,
            outputLength: KeySize);

        var result = Convert.ToBase64String(Combine(salt, hash));
        return result;
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        var expectedHash = new byte[KeySize];
        Array.Copy(hashBytes, SaltSize, expectedHash, 0, KeySize);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithm,
            outputLength: KeySize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] Combine(byte[] salt, byte[] hash)
    {
        var result = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);
        return result;
    }
}
