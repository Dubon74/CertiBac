using System.Data;
using TriviaApp.Models;

namespace TriviaApp.Data;

public interface ITriviaRepository
{
    Task<IReadOnlyList<Categoria>> ListarCategoriasAsync();

    Task<PartidaIniciada> IniciarPartidaAsync(int usuarioId, int categoriaId);

    Task<ResultadoRespuesta> ResponderAsync(int usuarioId, int partidaId, int preguntaId, int respuestaId);

    Task<IReadOnlyList<PosicionTablero>> TableroAsync(int top);
}

public sealed class TriviaRepository : ITriviaRepository
{
    private readonly IStoredProcedureRunner _db;

    public TriviaRepository(IStoredProcedureRunner db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Categoria>> ListarCategoriasAsync()
    {
        return await _db.QueryAsync(
            "dbo.usp_ListarCategorias",
            r => new Categoria(r.Int("Id"), r.Str("Nombre"), r.NullableStr("Descripcion"), r.Int("TotalPreguntas")));
    }

    public async Task<PartidaIniciada> IniciarPartidaAsync(int usuarioId, int categoriaId)
    {
        // El procedimiento devuelve una fila por cada respuesta (3 preguntas x 3 respuestas).
        var filas = await _db.QueryAsync(
            "dbo.usp_IniciarPartida",
            r => new
            {
                PartidaId = r.Int("PartidaId"),
                Orden = r.Int("Orden"),
                PreguntaId = r.Int("PreguntaId"),
                PreguntaTexto = r.Str("PreguntaTexto"),
                RespuestaId = r.Int("RespuestaId"),
                RespuestaTexto = r.Str("RespuestaTexto")
            },
            Sql.Param("@UsuarioId", SqlDbType.Int, usuarioId),
            Sql.Param("@CategoriaId", SqlDbType.Int, categoriaId));

        if (filas.Count == 0)
        {
            throw new InvalidOperationException("usp_IniciarPartida no devolvió preguntas.");
        }

        var preguntas = filas
            .GroupBy(f => new { f.Orden, f.PreguntaId, f.PreguntaTexto })
            .OrderBy(g => g.Key.Orden)
            .Select(g => new PreguntaPartida(
                g.Key.PreguntaId,
                g.Key.PreguntaTexto,
                g.Select(f => new RespuestaPartida(f.RespuestaId, f.RespuestaTexto)).ToList()))
            .ToList();

        return new PartidaIniciada(filas[0].PartidaId, preguntas);
    }

    public async Task<ResultadoRespuesta> ResponderAsync(int usuarioId, int partidaId, int preguntaId, int respuestaId)
    {
        var resultado = await _db.QuerySingleOrDefaultAsync(
            "dbo.usp_ResponderPregunta",
            r => new ResultadoRespuesta(
                r.Int("Codigo"),
                r.Str("Mensaje"),
                r.NullableBool("EsCorrecta"),
                r.NullableInt("RespuestaCorrectaId") ?? 0,
                r.NullableBool("Finalizada"),
                r.Int("Puntaje"),
                r.Int("Respondidas")),
            Sql.Param("@PartidaId", SqlDbType.Int, partidaId),
            Sql.Param("@UsuarioId", SqlDbType.Int, usuarioId),
            Sql.Param("@PreguntaId", SqlDbType.Int, preguntaId),
            Sql.Param("@RespuestaId", SqlDbType.Int, respuestaId));

        return resultado ?? new ResultadoRespuesta(99, "No se obtuvo respuesta de la base de datos.", false, 0, false, 0, 0);
    }

    public async Task<IReadOnlyList<PosicionTablero>> TableroAsync(int top)
    {
        return await _db.QueryAsync(
            "dbo.usp_TableroPosiciones",
            r => new PosicionTablero(
                r.Int("Posicion"),
                r.Str("NombreUsuario"),
                r.Int("PartidasJugadas"),
                r.Int("PuntajeTotal"),
                r.Int("Efectividad")),
            Sql.Param("@Top", SqlDbType.Int, top));
    }
}
