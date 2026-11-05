using System.Security.Cryptography;

namespace TriviaApp.Services;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string storedHash);
}

/// <summary>
/// PBKDF2-HMAC-SHA256 con sal aleatoria por usuario. Viene incluido en .NET
/// (no requiere paquetes). Formato guardado: PBKDF2$iteraciones$sal$hash
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const string Prefix = "PBKDF2";

    private readonly int _iterations;

    // 600.000 iteraciones: recomendación OWASP para PBKDF2-HMAC-SHA256.
    // Las pruebas unitarias usan un valor menor para ir rápido.
    public Pbkdf2PasswordHasher(int iterations = 600_000)
    {
        _iterations = iterations;
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Prefix}${_iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Comparación en tiempo constante (evita ataques por temporización).
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
