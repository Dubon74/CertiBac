using TriviaApp.Data;
using TriviaApp.Models;

namespace TriviaApp.Services;

/// <summary>Reglas de registro e inicio de sesión (validación en servidor).</summary>
public sealed class AuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    public async Task<RegistroResultado> RegistrarAsync(string? usuario, string? password)
    {
        usuario = usuario?.Trim();

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            return new RegistroResultado(1, "Usuario y contraseña son obligatorios.", null);
        }

        // La existencia del usuario la valida el procedimiento almacenado (y el UNIQUE de la tabla).
        return await _usuarios.RegistrarAsync(usuario, _hasher.Hash(password));
    }

    /// <returns>El usuario si las credenciales son válidas; de lo contrario null.</returns>
    public async Task<Usuario?> ValidarCredencialesAsync(string? usuario, string? password)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var encontrado = await _usuarios.ObtenerParaLoginAsync(usuario.Trim());

        return encontrado is not null && _hasher.Verify(password, encontrado.PasswordHash)
            ? encontrado
            : null;
    }
}
