using System.Text.Json;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    public class JsonLogService : IJsonLogService
    {
        private readonly string _logPath;

        public JsonLogService(IWebHostEnvironment environment)
        {
            var logsFolder = Path.Combine(environment.ContentRootPath, "Logs");
            Directory.CreateDirectory(logsFolder);
            _logPath = Path.Combine(logsFolder, "usuarios-log.txt");
        }

        public async Task LogUsuarioAsync(Usuario usuario)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                usuario.Id,
                usuario.Nombre,
                usuario.Correo,
                usuario.FechaDeNacimiento,
                usuario.FechaDeRegistro
            };

            var jsonLine = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(_logPath, jsonLine + Environment.NewLine);
        }

        public async Task<string> ReadLogsAsync()
        {
            if (!File.Exists(_logPath))
                return "[]";

            var lines = await File.ReadAllLinesAsync(_logPath);

            var validLines = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (validLines.Length == 0)
                return "[]";

            return "[" + string.Join(",", validLines) + "]";
        }
    }
}