using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Usuarios;

namespace WebApi.Services;

public class UsuariosService
{
    private readonly RestauranteDbContext _context;
    private readonly HashService _hashService;

    public UsuariosService(RestauranteDbContext context, HashService hashService)
    {
        _context = context;
        _hashService = hashService;
    }

    public async Task<List<UsuarioDto>> GetUsuariosAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Modulos)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return usuarios.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto?> GetUsuarioByIdAsync(string id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            return null;

        return MapToDto(usuario);
    }

    public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        var existente = await _context.Usuarios.AnyAsync(u => u.Username == dto.Username);
        if (existente)
            throw new InvalidOperationException("El username ya existe");

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Pin, @"^\d{4,8}$"))
            throw new InvalidOperationException("El PIN debe ser de 4 a 8 dígitos numéricos");

        var rolesValidos = new[] { "admin", "supervisor", "mesero", "cocina", "caja" };
        if (!rolesValidos.Contains(dto.Rol))
            throw new InvalidOperationException("Rol inválido");

        var pinHash = _hashService.HashPin(dto.Pin);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = dto.Nombre,
            Username = dto.Username,
            PinHash = pinHash,
            RolId = dto.Rol,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var modulos = await _context.Modulos
            .Where(m => dto.Modules.Contains(m.Id))
            .ToListAsync();

        usuario.Modulos = modulos;
        await _context.SaveChangesAsync();

        return MapToDto(usuario);
    }

    public async Task<UsuarioDto?> UpdateUsuarioAsync(string id, UpdateUsuarioDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            usuario.Nombre = dto.Nombre;

        if (!string.IsNullOrWhiteSpace(dto.Username))
        {
            var existente = await _context.Usuarios.AnyAsync(u => u.Username == dto.Username && u.Id != id);
            if (existente)
                throw new InvalidOperationException("El username ya existe");

            usuario.Username = dto.Username;
        }

        if (!string.IsNullOrWhiteSpace(dto.Pin))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Pin, @"^\d{4,8}$"))
                throw new InvalidOperationException("El PIN debe ser de 4 a 8 dígitos numéricos");

            usuario.PinHash = _hashService.HashPin(dto.Pin);
        }

        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            var rolesValidos = new[] { "admin", "supervisor", "mesero", "cocina", "caja" };
            if (!rolesValidos.Contains(dto.Rol))
                throw new InvalidOperationException("Rol inválido");

            usuario.RolId = dto.Rol;
        }

        if (dto.Activo.HasValue)
            usuario.Activo = dto.Activo.Value;

        if (dto.Modules != null)
        {
            usuario.Modulos.Clear();
            var modulos = await _context.Modulos
                .Where(m => dto.Modules.Contains(m.Id))
                .ToListAsync();
            usuario.Modulos = modulos;
        }

        await _context.SaveChangesAsync();

        return MapToDto(usuario);
    }

    public async Task<bool> DeleteUsuarioAsync(string id, string currentUserId)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("No puedes eliminar tu propio usuario");

        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return false;

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UsuarioDto?> VerificarPinSupervisorAsync(string pin)
    {
        var pinHash = _hashService.HashPin(pin);

        var usuario = await _context.Usuarios
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => 
                u.PinHash == pinHash && 
                u.Activo && 
                (u.RolId == "admin" || u.RolId == "supervisor"));

        if (usuario == null)
            return null;

        return MapToDto(usuario);
    }

    private UsuarioDto MapToDto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Username = usuario.Username,
            Rol = usuario.RolId,
            Activo = usuario.Activo,
            Modules = usuario.Modulos.Select(m => m.Id).ToList(),
            CreadoEn = null
        };
    }
}
