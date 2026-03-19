using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.DTOs;
using ProyectoFinal.Models;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IUserLogService _userLogService;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext context,
        TokenService tokenService,
        IUserLogService userLogService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _userLogService = userLogService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UsuarioResponseDto>> Register([FromBody] Usuario usuario)
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

        return CreatedAtAction(
            nameof(UsuariosController.GetById),
            "Usuarios",
            new { id = usuario.Id },
            new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                FechaDeNacimiento = usuario.FechaDeNacimiento,
                FechaDeRegistro = usuario.FechaDeRegistro
            });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

        if (usuario is null || !_tokenService.VerifyPassword(request.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var accessToken = _tokenService.GenerateAccessToken(usuario);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "30"));

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return Ok(new TokenResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = expiration,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if (usuario is null)
        {
            return Unauthorized(new { message = "Refresh token inválido." });
        }

        if (!usuario.RefreshTokenExpiryTime.HasValue || usuario.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Refresh token expirado." });
        }

        var newAccessToken = _tokenService.GenerateAccessToken(usuario);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "30"));

        usuario.RefreshToken = newRefreshToken;
        usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _context.SaveChangesAsync();

        return Ok(new TokenResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = expiration,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo
        });
    }
}
