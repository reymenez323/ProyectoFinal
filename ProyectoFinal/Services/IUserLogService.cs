using ProyectoFinal.DTOs;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

public interface IUserLogService
{
    Task AppendUserRegistrationLogAsync(Usuario usuario);
    Task<List<UserLogEntryDto>> GetLogsAsync();
}
