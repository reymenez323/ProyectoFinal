using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
