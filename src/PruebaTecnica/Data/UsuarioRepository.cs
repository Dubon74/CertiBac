using System.Data;
using TriviaApp.Models;

namespace TriviaApp.Data;

public interface IUsuarioRepository
{
    Task<RegistroResultado> RegistrarAsync(string nombreUsuario, string passwordHash);

    Task<Usuario?> ObtenerParaLoginAsync(string nombreUsuario);
}

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IStoredProcedureRunner _db;

    public UsuarioRepository(IStoredProcedureRunner db)
    {
        _db = db;
    }

    public async Task<RegistroResultado> RegistrarAsync(string nombreUsuario, string passwordHash)
    {
        var resultado = await _db.QuerySingleOrDefaultAsync(
            "dbo.usp_RegistrarUsuario",
            r => new RegistroResultado(r.Int("Codigo"), r.Str("Mensaje"), r.NullableInt("UsuarioId")),
            Sql.Param("@NombreUsuario", SqlDbType.NVarChar, nombreUsuario, 50),
            Sql.Param("@PasswordHash", SqlDbType.NVarChar, passwordHash, 200));

        return resultado ?? new RegistroResultado(99, "No se obtuvo respuesta de la base de datos.", null);
    }

    public Task<Usuario?> ObtenerParaLoginAsync(string nombreUsuario)
    {
        return _db.QuerySingleOrDefaultAsync(
            "dbo.usp_ObtenerUsuarioLogin",
            r => new Usuario(r.Int("Id"), r.Str("NombreUsuario"), r.Str("PasswordHash")),
            Sql.Param("@NombreUsuario", SqlDbType.NVarChar, nombreUsuario, 50));
    }
}
