using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}