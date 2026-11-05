using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriviaApp.Services;

namespace TriviaApp.Controllers;

public class HomeController : Controller
{
    private readonly TriviaService _trivia;

    public HomeController(TriviaService trivia)
    {
        _trivia = trivia;
    }

    // Pestaña "Inicio": categorías y partida.
    public async Task<IActionResult> Index()
    {
        ViewData["Tab"] = "inicio";
        return View(await _trivia.ListarCategoriasAsync());
    }

    // Pestaña "Tablero de posiciones".
    public async Task<IActionResult> Tablero()
    {
        ViewData["Tab"] = "tablero";
        return View(await _trivia.TableroAsync());
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
