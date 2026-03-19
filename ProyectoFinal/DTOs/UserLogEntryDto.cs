namespace ProyectoFinal.DTOs;

public class UserLogEntryDto
{
    public DateTime FechaUtc { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaDeNacimiento { get; set; }
    public string Evento { get; set; } = "UsuarioRegistrado";
}
