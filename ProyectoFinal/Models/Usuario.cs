using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public DateTime FechaDeNacimiento { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 6)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
