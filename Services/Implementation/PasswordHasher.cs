using System.Security.Cryptography;
using Services.Interface;

namespace Services.Implementation;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 16 bytes
    private const int KeySize = 256 / 8; // 32 bytes
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA256;
    private const char Delimiter = ':';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName,
            KeySize);

        return string.Join(
            Delimiter,
            Convert.ToBase64String(hash),
            Convert.ToBase64String(salt),
            Iterations,
            HashAlgorithmName);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var elements = hashedPassword.Split(Delimiter);
        if (elements.Length != 4)
        {
            return false;
        }

        var hash = Convert.FromBase64String(elements[0]);
        var salt = Convert.FromBase64String(elements[1]);
        var iterations = int.Parse(elements[2]);
        var hashAlgorithmName = new HashAlgorithmName(elements[3]);

        var hashToCheck = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            hashAlgorithmName,
            hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, hashToCheck);
    }
}