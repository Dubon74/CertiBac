using System.Security.Claims;

namespace TriviaApp.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("La sesión no contiene un identificador de usuario válido.");
    }
}
