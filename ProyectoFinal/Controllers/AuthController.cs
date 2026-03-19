using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.DTOs;
using ProyectoFinal.Models;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IJsonLogService _jsonLogService;

        public AuthController(
            AppDbContext context,
            IJwtService jwtService,
            IJsonLogService jsonLogService)
        {
            _context = context;
            _jwtService = jwtService;
            _jsonLogService = jsonLogService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == request.Correo);

            if (correoExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El correo electrónico ya está en uso."
                });
            }

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Correo = request.Correo,
                FechaDeNacimiento = request.FechaDeNacimiento,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                FechaDeRegistro = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await _jsonLogService.LogUsuarioAsync(usuario);

            return Ok(new
            {
                mensaje = "Usuario registrado correctamente.",
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Correo,
                    usuario.FechaDeNacimiento,
                    usuario.FechaDeRegistro
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var passwordHash = PasswordHelper.HashPassword(request.Password);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == request.Correo &&
                    u.PasswordHash == passwordHash);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales inválidas."
                });
            }

            var token = _jwtService.GenerateAccessToken(usuario);
            var refreshToken = _jwtService.GenerateRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Refresh token inválido."
                });
            }

            if (!usuario.RefreshTokenExpiryTime.HasValue ||
                usuario.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                return Unauthorized(new
                {
                    mensaje = "El refresh token ha expirado."
                });
            }

            var newAccessToken = _jwtService.GenerateAccessToken(usuario);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            usuario.RefreshToken = newRefreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}