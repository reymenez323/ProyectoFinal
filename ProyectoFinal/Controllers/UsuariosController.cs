using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJsonLogService _jsonLogService;

        public UsuariosController(AppDbContext context, IJsonLogService jsonLogService)
        {
            _context = context;
            _jsonLogService = jsonLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Correo,
                    u.FechaDeNacimiento,
                    u.FechaDeRegistro
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Correo,
                    u.FechaDeNacimiento,
                    u.FechaDeRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(usuario);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _jsonLogService.ReadLogsAsync();
            return Content(logs, "application/json");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario eliminado correctamente." });
        }
    }
}