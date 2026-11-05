namespace TriviaApp.Models;

public sealed record Usuario(int Id, string NombreUsuario, string PasswordHash);

/// <summary>Codigo: 0 = OK, 1 = campos vacios, 2 = usuario ya existe.</summary>
public sealed record RegistroResultado(int Codigo, string Mensaje, int? UsuarioId)
{
    public bool Exitoso => Codigo == 0;
}

public sealed record Categoria(int Id, string Nombre, string? Descripcion, int TotalPreguntas);

/// <summary>Respuesta mostrada al jugador. Nunca indica si es la correcta.</summary>
public sealed record RespuestaPartida(int Id, string Texto);

public sealed record PreguntaPartida(int Id, string Texto, IReadOnlyList<RespuestaPartida> Respuestas);

public sealed record PartidaIniciada(int PartidaId, IReadOnlyList<PreguntaPartida> Preguntas);

/// <summary>Resultado de calificar una respuesta (se calcula en el servidor).</summary>
public sealed record ResultadoRespuesta(
    int Codigo,
    string Mensaje,
    bool EsCorrecta,
    int RespuestaCorrectaId,
    bool Finalizada,
    int Puntaje,
    int Respondidas);

public sealed record PosicionTablero(
    int Posicion,
    string NombreUsuario,
    int PartidasJugadas,
    int PuntajeTotal,
    int Efectividad);
