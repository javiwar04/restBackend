using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Mesas;

namespace WebApi.Services;

public class MesasService
{
    private readonly RestauranteDbContext _context;

    public MesasService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<SeccionDto>> GetSeccionesAsync(string? establecimientoId = null)
    {
        var query = _context.Secciones.Include(s => s.Mesas).AsQueryable();

        // Solo las secciones/mesas de la sucursal activa
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(s => s.EstablecimientoId == establecimientoId);

        var secciones = await query.OrderBy(s => s.Orden).ToListAsync();

        return secciones.Select(s => new SeccionDto
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Orden = s.Orden,
            Activa = s.Activa,
            EstablecimientoId = s.EstablecimientoId,
            Mesas = s.Mesas.Select(m => new MesaDto
            {
                Id = m.Id,
                Numero = m.Numero,
                Etiqueta = m.Etiqueta,
                Capacidad = m.Capacidad,
                SeccionId = m.SeccionId,
                Activa = m.Activa,
                Notas = m.Notas
            }).OrderBy(m => m.Numero).ToList()
        }).ToList();
    }

    public async Task<List<MesaDto>> GetMesasAsync(string? seccionId = null, bool? activa = null, string? establecimientoId = null)
    {
        var query = _context.Mesas
            .Include(m => m.Seccion)
            .AsQueryable();

        // La mesa hereda la sucursal de su sección
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(m => m.Seccion.EstablecimientoId == establecimientoId);

        if (!string.IsNullOrEmpty(seccionId))
            query = query.Where(m => m.SeccionId == seccionId);

        if (activa.HasValue)
            query = query.Where(m => m.Activa == activa.Value);

        var mesas = await query
            .OrderBy(m => m.Numero)
            .ToListAsync();

        return mesas.Select(m => new MesaDto
        {
            Id = m.Id,
            Numero = m.Numero,
            Etiqueta = m.Etiqueta,
            Capacidad = m.Capacidad,
            SeccionId = m.SeccionId,
            SeccionNombre = m.Seccion.Nombre,
            Activa = m.Activa,
            Notas = m.Notas
        }).ToList();
    }

    public async Task<MesaDto> CreateMesaAsync(CreateMesaDto dto)
    {
        var seccion = await _context.Secciones.FindAsync(dto.SeccionId);
        if (seccion == null)
            throw new InvalidOperationException("Secci�n no encontrada");

        var mesa = new Mesa
        {
            Id = Guid.NewGuid().ToString(),
            Numero = dto.Numero,
            Etiqueta = dto.Etiqueta,
            Capacidad = dto.Capacidad,
            SeccionId = dto.SeccionId,
            Activa = dto.Activa,
            Notas = dto.Notas
        };

        _context.Mesas.Add(mesa);
        await _context.SaveChangesAsync();

        return new MesaDto
        {
            Id = mesa.Id,
            Numero = mesa.Numero,
            Etiqueta = mesa.Etiqueta,
            Capacidad = mesa.Capacidad,
            SeccionId = mesa.SeccionId,
            SeccionNombre = seccion.Nombre,
            Activa = mesa.Activa,
            Notas = mesa.Notas
        };
    }

    public async Task<MesaDto?> UpdateMesaAsync(string id, UpdateMesaDto dto)
    {
        var mesa = await _context.Mesas
            .Include(m => m.Seccion)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mesa == null)
            return null;

        var seccion = await _context.Secciones.FindAsync(dto.SeccionId);
        if (seccion == null)
            throw new InvalidOperationException("Secci�n no encontrada");

        mesa.Numero = dto.Numero;
        mesa.Etiqueta = dto.Etiqueta;
        mesa.Capacidad = dto.Capacidad;
        mesa.SeccionId = dto.SeccionId;
        mesa.Activa = dto.Activa;
        mesa.Notas = dto.Notas;

        await _context.SaveChangesAsync();

        return new MesaDto
        {
            Id = mesa.Id,
            Numero = mesa.Numero,
            Etiqueta = mesa.Etiqueta,
            Capacidad = mesa.Capacidad,
            SeccionId = mesa.SeccionId,
            SeccionNombre = seccion.Nombre,
            Activa = mesa.Activa,
            Notas = mesa.Notas
        };
    }

    public async Task<bool> DeleteMesaAsync(string id)
    {
        var mesa = await _context.Mesas.FindAsync(id);

        if (mesa == null)
            return false;

        _context.Mesas.Remove(mesa);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SeccionDto> CreateSeccionAsync(CreateSeccionDto dto, string? establecimientoId)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre es requerido");

        if (string.IsNullOrEmpty(establecimientoId))
            throw new ArgumentException("Seleccione una sucursal");

        // Nombre unico DENTRO de la misma sucursal
        var existe = await _context.Secciones
            .AnyAsync(s => s.EstablecimientoId == establecimientoId && s.Nombre.ToLower() == dto.Nombre.ToLower());

        if (existe)
            throw new InvalidOperationException("Ya existe una secci�n con ese nombre en esta sucursal");

        var seccion = new Seccione
        {
            Id = Guid.NewGuid().ToString(),
            EstablecimientoId = establecimientoId,
            Nombre = dto.Nombre,
            Orden = dto.Orden,
            Activa = dto.Activa
        };

        _context.Secciones.Add(seccion);
        await _context.SaveChangesAsync();

        return new SeccionDto
        {
            Id = seccion.Id,
            Nombre = seccion.Nombre,
            Orden = seccion.Orden,
            Activa = seccion.Activa,
            Mesas = new List<MesaDto>()
        };
    }

    public async Task<SeccionDto?> UpdateSeccionAsync(string id, UpdateSeccionDto dto)
    {
        var seccion = await _context.Secciones
            .Include(s => s.Mesas)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (seccion == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != seccion.Nombre)
        {
            var existe = await _context.Secciones
                .AnyAsync(s => s.EstablecimientoId == seccion.EstablecimientoId && s.Nombre.ToLower() == dto.Nombre.ToLower() && s.Id != id);

            if (existe)
                throw new InvalidOperationException("Ya existe una secci�n con ese nombre en esta sucursal");

            seccion.Nombre = dto.Nombre;
        }

        if (dto.Orden.HasValue)
            seccion.Orden = dto.Orden.Value;

        if (dto.Activa.HasValue)
            seccion.Activa = dto.Activa.Value;

        await _context.SaveChangesAsync();

        return new SeccionDto
        {
            Id = seccion.Id,
            Nombre = seccion.Nombre,
            Orden = seccion.Orden,
            Activa = seccion.Activa,
            Mesas = seccion.Mesas.Select(m => new MesaDto
            {
                Id = m.Id,
                Numero = m.Numero,
                Etiqueta = m.Etiqueta,
                Capacidad = m.Capacidad,
                SeccionId = m.SeccionId,
                Activa = m.Activa,
                Notas = m.Notas
            }).OrderBy(m => m.Numero).ToList()
        };
    }

    public async Task<bool> DeleteSeccionAsync(string id)
    {
        var seccion = await _context.Secciones
            .Include(s => s.Mesas)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (seccion == null)
            return false;

        if (seccion.Mesas.Any())
            throw new InvalidOperationException("La secci�n tiene mesas asignadas. Elimina o reasigna las mesas primero.");

        _context.Secciones.Remove(seccion);
        await _context.SaveChangesAsync();

        return true;
    }
}
