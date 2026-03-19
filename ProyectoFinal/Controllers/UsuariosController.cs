using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.DTOs;
using ProyectoFinal.Models;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IUserLogService _userLogService;

    public UsuariosController(AppDbContext context, TokenService tokenService, IUserLogService userLogService)
    {
        _context = context;
        _tokenService = tokenService;
        _userLogService = userLogService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetAll()
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Correo = u.Correo,
                FechaDeNacimiento = u.FechaDeNacimiento,
                FechaDeRegistro = u.FechaDeRegistro
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Correo = u.Correo,
                FechaDeNacimiento = u.FechaDeNacimiento,
                FechaDeRegistro = u.FechaDeRegistro
            })
            .FirstOrDefaultAsync();

        if (usuario is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(usuario);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Create([FromBody] Usuario usuario)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        usuario.Correo = usuario.Correo.Trim().ToLowerInvariant();

        var exists = await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo);
        if (exists)
        {
            return BadRequest(new { message = "El correo electrónico ya está en uso." });
        }

        usuario.PasswordHash = _tokenService.HashPassword(usuario.PasswordHash);
        usuario.FechaDeRegistro = DateTime.UtcNow;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        await _userLogService.AppendUserRegistrationLogAsync(usuario);

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, new UsuarioResponseDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            FechaDeNacimiento = usuario.FechaDeNacimiento,
            FechaDeRegistro = usuario.FechaDeRegistro
        });
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Usuario input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var correoNormalizado = input.Correo.Trim().ToLowerInvariant();
        var correoDuplicado = await _context.Usuarios.AnyAsync(u => u.Correo == correoNormalizado && u.Id != id);
        if (correoDuplicado)
        {
            return BadRequest(new { message = "El correo electrónico ya está en uso." });
        }

        usuario.Nombre = input.Nombre;
        usuario.Correo = correoNormalizado;
        usuario.FechaDeNacimiento = input.FechaDeNacimiento;

        if (!string.IsNullOrWhiteSpace(input.PasswordHash))
        {
            usuario.PasswordHash = _tokenService.HashPassword(input.PasswordHash);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _userLogService.GetLogsAsync();

        if (logs.Count == 0)
        {
            return Ok(new { message = "No hay logs registrados.", logs });
        }

        return Ok(logs);
    }
}
