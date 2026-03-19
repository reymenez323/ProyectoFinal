namespace ProyectoFinal.DTOs;

public class UsuarioResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaDeNacimiento { get; set; }
    public DateTime FechaDeRegistro { get; set; }
}
