using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
