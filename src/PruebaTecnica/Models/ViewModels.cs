using System.ComponentModel.DataAnnotations;

namespace TriviaApp.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string? Usuario { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Solo se permiten letras, números, punto, guion y guion bajo.")]
    [Display(Name = "Usuario")]
    public string? Usuario { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Debes confirmar la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarPassword { get; set; }
}

public sealed class IniciarPartidaRequest
{
    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }
}

public sealed class ResponderRequest
{
    [Range(1, int.MaxValue)]
    public int PartidaId { get; set; }

    [Range(1, int.MaxValue)]
    public int PreguntaId { get; set; }

    [Range(1, int.MaxValue)]
    public int RespuestaId { get; set; }
}