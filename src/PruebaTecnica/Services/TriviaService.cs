using Microsoft.Data.SqlClient;
using TriviaApp.Data;
using TriviaApp.Models;

namespace TriviaApp.Services;

/// <summary>Error de negocio que se puede mostrar al jugador.</summary>
public sealed class TriviaException : Exception
{
    public TriviaException(string message) : base(message)
    {
    }
}

public sealed class TriviaService
{
    private const int TopTablero = 10;

    private readonly ITriviaRepository _repo;

    public TriviaService(ITriviaRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<Categoria>> ListarCategoriasAsync() => _repo.ListarCategoriasAsync();

    public async Task<PartidaIniciada> IniciarPartidaAsync(int usuarioId, int categoriaId)
    {
        try
        {
            return await _repo.IniciarPartidaAsync(usuarioId, categoriaId);
        }
        catch (SqlException ex) when (ex.Number is >= 50001 and <= 50003)
        {
            // Errores dentro del procedimiento almacenado.
            throw new TriviaException(ex.Message);
        }
    }

    public async Task<ResultadoRespuesta> ResponderAsync(int usuarioId, int partidaId, int preguntaId, int respuestaId)
    {
        var resultado = await _repo.ResponderAsync(usuarioId, partidaId, preguntaId, respuestaId);

        if (resultado.Codigo != 0)
        {
            throw new TriviaException(resultado.Mensaje);
        }

        return resultado;
    }

    public Task<IReadOnlyList<PosicionTablero>> TableroAsync() => _repo.TableroAsync(TopTablero);
}
