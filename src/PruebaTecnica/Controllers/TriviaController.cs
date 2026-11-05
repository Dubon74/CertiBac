using Microsoft.AspNetCore.Mvc;
using TriviaApp.Infrastructure;
using TriviaApp.Models;
using TriviaApp.Services;

namespace TriviaApp.Controllers;

/// <summary>
/// Endpoints JSON que consume el navegador por AJAX durante la partida.
/// Requieren sesión (política por defecto) y token anti-CSRF (filtro global).
/// </summary>
[ApiController]
[Route("api/trivia")]
public class TriviaController : ControllerBase
{
    private readonly TriviaService _trivia;

    public TriviaController(TriviaService trivia)
    {
        _trivia = trivia;
    }

    [HttpPost("iniciar")]
    public async Task<IActionResult> Iniciar([FromBody] IniciarPartidaRequest request)
    {
        try
        {
            var partida = await _trivia.IniciarPartidaAsync(User.GetUserId(), request.CategoriaId);
            return Ok(partida);
        }
        catch (TriviaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("responder")]
    public async Task<IActionResult> Responder([FromBody] ResponderRequest request)
    {
        try
        {
            var resultado = await _trivia.ResponderAsync(
                User.GetUserId(), request.PartidaId, request.PreguntaId, request.RespuestaId);
            return Ok(resultado);
        }
        catch (TriviaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
