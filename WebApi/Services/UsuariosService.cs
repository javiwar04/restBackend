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

        // Sucursales asignadas a cada usuario (tabla puente), en un solo query
        var ids = usuarios.Select(u => u.Id).ToList();
        var porUsuario = (await _context.UsuariosEstablecimientos
                .Where(ue => ids.Contains(ue.UsuarioId))
                .ToListAsync())
            .GroupBy(ue => ue.UsuarioId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.EstablecimientoId).ToList());

        return usuarios
            .Select(u => MapToDto(u, porUsuario.TryGetValue(u.Id, out var l) ? l : new List<string>()))
            .ToList();
    }

    public async Task<UsuarioDto?> GetUsuarioByIdAsync(string id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Modulos)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            return null;

        var establecimientoIds = await _context.UsuariosEstablecimientos
            .Where(ue => ue.UsuarioId == id)
            .Select(ue => ue.EstablecimientoId)
            .ToListAsync();

        return MapToDto(usuario, establecimientoIds);
    }

    public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        var existente = await _context.Usuarios.AnyAsync(u => u.Username == dto.Username);
        if (existente)
            throw new InvalidOperationException("El username ya existe");

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Pin, @"^\d{4,8}$"))
            throw new InvalidOperationException("El PIN debe ser de 4 a 8 d�gitos num�ricos");

        var rolesValidos = new[] { "admin", "supervisor", "mesero", "cocina", "caja" };
        if (!rolesValidos.Contains(dto.Rol))
            throw new InvalidOperationException("Rol inv�lido");

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

        // Sucursales asignadas (tabla puente)
        foreach (var estId in dto.EstablecimientoIds.Distinct())
            _context.UsuariosEstablecimientos.Add(new UsuarioEstablecimiento { UsuarioId = usuario.Id, EstablecimientoId = estId });

        await _context.SaveChangesAsync();

        return MapToDto(usuario, dto.EstablecimientoIds);
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
                throw new InvalidOperationException("El PIN debe ser de 4 a 8 d�gitos num�ricos");

            usuario.PinHash = _hashService.HashPin(dto.Pin);
        }

        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            var rolesValidos = new[] { "admin", "supervisor", "mesero", "cocina", "caja" };
            if (!rolesValidos.Contains(dto.Rol))
                throw new InvalidOperationException("Rol inv�lido");

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

        // Sincronizar sucursales asignadas (reemplaza el set completo)
        if (dto.EstablecimientoIds != null)
        {
            var actuales = await _context.UsuariosEstablecimientos
                .Where(ue => ue.UsuarioId == id)
                .ToListAsync();
            _context.UsuariosEstablecimientos.RemoveRange(actuales);
            await _context.SaveChangesAsync(); // borrar antes de reinsertar (PK compuesta)

            foreach (var estId in dto.EstablecimientoIds.Distinct())
                _context.UsuariosEstablecimientos.Add(new UsuarioEstablecimiento { UsuarioId = id, EstablecimientoId = estId });
        }

        await _context.SaveChangesAsync();

        var finalIds = dto.EstablecimientoIds ?? await _context.UsuariosEstablecimientos
            .Where(ue => ue.UsuarioId == id)
            .Select(ue => ue.EstablecimientoId)
            .ToListAsync();

        return MapToDto(usuario, finalIds);
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

    private UsuarioDto MapToDto(Usuario usuario, List<string>? establecimientoIds = null)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Username = usuario.Username,
            Rol = usuario.RolId,
            Activo = usuario.Activo,
            Modules = usuario.Modulos.Select(m => m.Id).ToList(),
            EstablecimientoIds = establecimientoIds ?? new List<string>(),
            CreadoEn = null
        };
    }
}
