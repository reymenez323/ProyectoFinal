using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    public interface IJsonLogService
    {
        Task LogUsuarioAsync(Usuario usuario);
        Task<string> ReadLogsAsync();
    }
}