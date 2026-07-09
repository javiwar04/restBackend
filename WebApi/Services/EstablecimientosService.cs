using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Establecimientos;

namespace WebApi.Services;

public class EstablecimientosService
{
    private readonly RestauranteDbContext _context;

    public EstablecimientosService(RestauranteDbContext context)
    {
        _context = context;
    }

    private static EstablecimientoDto Map(Establecimiento e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Direccion = e.Direccion,
        Telefono = e.Telefono,
        Activo = e.Activo
    };

    /// <summary>
    /// Establecimientos que puede usar el usuario. El admin ve todos los
    /// activos; el resto solo los asignados en el puente.
    /// </summary>
    public async Task<List<EstablecimientoDto>> GetForUserAsync(string usuarioId, bool esAdmin)
    {
        var query = _context.Establecimientos.Where(e => e.Activo);

        if (!esAdmin)
        {
            query = query.Where(e => _context.UsuariosEstablecimientos
                .Any(ue => ue.UsuarioId == usuarioId && ue.EstablecimientoId == e.Id));
        }

        return await query.OrderBy(e => e.Nombre).Select(e => Map(e)).ToListAsync();
    }

    public async Task<List<EstablecimientoDto>> GetAllAsync()
    {
        return await _context.Establecimientos
            .OrderBy(e => e.Nombre)
            .Select(e => Map(e))
            .ToListAsync();
    }

    public async Task<EstablecimientoDto> CreateAsync(CreateEstablecimientoDto dto)
    {
        var est = new Establecimiento
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = dto.Nombre.Trim(),
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };
        _context.Establecimientos.Add(est);
        await _context.SaveChangesAsync();
        return Map(est);
    }

    public async Task<EstablecimientoDto?> UpdateAsync(string id, UpdateEstablecimientoDto dto)
    {
        var est = await _context.Establecimientos.FindAsync(id);
        if (est == null) return null;

        if (dto.Nombre != null) est.Nombre = dto.Nombre.Trim();
        if (dto.Direccion != null) est.Direccion = dto.Direccion;
        if (dto.Telefono != null) est.Telefono = dto.Telefono;
        if (dto.Activo.HasValue) est.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();
        return Map(est);
    }

    /// <summary>Verifica que el usuario tenga acceso al establecimiento (admin siempre).</summary>
    public async Task<bool> UsuarioTieneAccesoAsync(string usuarioId, bool esAdmin, string establecimientoId)
    {
        if (esAdmin) return await _context.Establecimientos.AnyAsync(e => e.Id == establecimientoId);
        return await _context.UsuariosEstablecimientos
            .AnyAsync(ue => ue.UsuarioId == usuarioId && ue.EstablecimientoId == establecimientoId);
    }
}
