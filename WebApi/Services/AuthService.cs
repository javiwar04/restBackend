using AccesoDatos.Context;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Auth;

namespace WebApi.Services;

public class AuthService
{
    private readonly RestauranteDbContext _context;
    private readonly HashService _hashService;
    private readonly JwtService _jwtService;

    public AuthService(RestauranteDbContext context, HashService hashService, JwtService jwtService)
    {
        _context = context;
        _hashService = hashService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string pin)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => u.Username == username && u.Activo);

        if (usuario == null)
            return null;

        if (!_hashService.VerifyPin(pin, usuario.PinHash))
            return null;

        var modules = usuario.Modulos.Select(m => m.Id).ToList();
        var token = _jwtService.GenerateToken(usuario.Id, usuario.Username, usuario.RolId, modules);

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.RolId,
                Modules = modules
            }
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => u.Id == userId && u.Activo);

        if (usuario == null)
            return null;

        return new UserDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Username = usuario.Username,
            Rol = usuario.RolId,
            Modules = usuario.Modulos.Select(m => m.Id).ToList()
        };
    }
}
