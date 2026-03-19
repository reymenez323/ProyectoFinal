using System.Text.Json;
using ProyectoFinal.DTOs;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

public class UserLogService : IUserLogService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public UserLogService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    private string GetLogPath()
    {
        var relativePath = _configuration["LogSettings:UserLogPath"] ?? "Logs/usuarios-log.txt";
        return Path.Combine(_environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    public async Task AppendUserRegistrationLogAsync(Usuario usuario)
    {
        var path = GetLogPath();
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, string.Empty);
        }

        var entry = new UserLogEntryDto
        {
            FechaUtc = DateTime.UtcNow,
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            FechaDeNacimiento = usuario.FechaDeNacimiento,
            Evento = "UsuarioRegistrado"
        };

        var json = JsonSerializer.Serialize(entry);
        await File.AppendAllTextAsync(path, json + Environment.NewLine);
    }

    public async Task<List<UserLogEntryDto>> GetLogsAsync()
    {
        var path = GetLogPath();

        if (!File.Exists(path))
        {
            return new List<UserLogEntryDto>();
        }

        var lines = await File.ReadAllLinesAsync(path);
        var result = new List<UserLogEntryDto>();

        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            try
            {
                var item = JsonSerializer.Deserialize<UserLogEntryDto>(line);
                if (item is not null)
                {
                    result.Add(item);
                }
            }
            catch
            {
            }
        }

        return result;
    }
}
