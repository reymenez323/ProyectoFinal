using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(Usuario usuario);
        string GenerateRefreshToken();
    }
}