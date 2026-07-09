using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Inventario;

namespace WebApi.Services;

public class InsumosService
{
    private readonly RestauranteDbContext _context;

    public InsumosService(RestauranteDbContext context)
    {
        _context = context;
    }

    // Mapeo comun a DTO
    private static InsumoDto Map(Insumo i) => new()
    {
        Id = i.Id,
        Nombre = i.Nombre,
        Unidad = i.Unidad,
        StockActual = i.StockActual,
        StockMinimo = i.StockMinimo,
        CostoUnitario = i.CostoPorUnidad,
        Activo = i.Activo,
        EstablecimientoId = i.EstablecimientoId,
        ActualizadoEn = i.CreadoEn
    };

    public async Task<List<InsumoDto>> GetInsumosAsync(string? establecimientoId = null)
    {
        var query = _context.Insumos.AsQueryable();

        // Si viene sucursal, solo sus insumos; si no, todos (vista admin)
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(i => i.EstablecimientoId == establecimientoId);

        var insumos = await query.OrderBy(i => i.Nombre).ToListAsync();
        return insumos.Select(Map).ToList();
    }

    public async Task<InsumoDto?> GetInsumoByIdAsync(string id)
    {
        var insumo = await _context.Insumos.FindAsync(id);
        return insumo == null ? null : Map(insumo);
    }

    public async Task<InsumoDto> CreateInsumoAsync(CreateInsumoDto dto, string? establecimientoId)
    {
        var insumo = new Insumo
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = dto.Nombre,
            Unidad = dto.Unidad,
            StockActual = dto.StockActual,
            StockMinimo = dto.StockMinimo,
            CostoPorUnidad = dto.CostoUnitario,
            EstablecimientoId = establecimientoId,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Insumos.Add(insumo);
        await _context.SaveChangesAsync();

        return Map(insumo);
    }

    public async Task<InsumoDto?> UpdateInsumoAsync(string id, UpdateInsumoDto dto)
    {
        var insumo = await _context.Insumos.FindAsync(id);

        if (insumo == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            insumo.Nombre = dto.Nombre;

        if (!string.IsNullOrWhiteSpace(dto.Unidad))
            insumo.Unidad = dto.Unidad;

        if (dto.StockActual.HasValue)
            insumo.StockActual = dto.StockActual.Value;

        if (dto.StockMinimo.HasValue)
            insumo.StockMinimo = dto.StockMinimo.Value;

        if (dto.CostoUnitario.HasValue)
            insumo.CostoPorUnidad = dto.CostoUnitario.Value;

        if (dto.Activo.HasValue)
            insumo.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();

        return Map(insumo);
    }

    public async Task<bool> DeleteInsumoAsync(string id)
    {
        var insumo = await _context.Insumos
            .Include(i => i.Receta)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (insumo == null)
            return false;

        if (insumo.Receta.Any())
            throw new InvalidOperationException("El insumo tiene recetas asociadas");

        _context.Insumos.Remove(insumo);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<InsumoDto?> AjusteStockAsync(string id, AjusteStockDto dto, string usuarioId)
    {
        var insumo = await _context.Insumos.FindAsync(id);

        if (insumo == null)
            return null;

        if (dto.Tipo == "entrada")
        {
            insumo.StockActual += dto.Cantidad;
        }
        else if (dto.Tipo == "salida")
        {
            if (insumo.StockActual < dto.Cantidad)
                throw new InvalidOperationException("Stock insuficiente para realizar la salida");

            insumo.StockActual -= dto.Cantidad;
        }
        else
        {
            throw new InvalidOperationException("Tipo inv�lido. Debe ser 'entrada' o 'salida'");
        }

        var movimiento = new InsumosMovimiento
        {
            InsumoId = id,
            Tipo = dto.Tipo,
            Cantidad = dto.Cantidad,
            CostoPorUnidad = insumo.CostoPorUnidad,
            Motivo = dto.Motivo,
            UsuarioId = usuarioId,
            RegistradoEn = DateTime.UtcNow
        };

        _context.InsumosMovimientos.Add(movimiento);
        await _context.SaveChangesAsync();

        return Map(insumo);
    }

    public async Task<List<MovimientoInsumoDto>> GetMovimientosAsync(
        string? insumoId = null,
        string? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limit = 200,
        string? establecimientoId = null)
    {
        limit = Math.Clamp(limit, 1, 1000);

        var query = _context.InsumosMovimientos
            .Include(m => m.Insumo)
            .AsQueryable();

        // Solo movimientos de insumos de la sucursal activa
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(m => m.Insumo.EstablecimientoId == establecimientoId);

        if (!string.IsNullOrEmpty(insumoId))
            query = query.Where(m => m.InsumoId == insumoId);

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(m => m.Tipo == tipo);

        if (desde.HasValue)
            query = query.Where(m => m.RegistradoEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => m.RegistradoEn <= hasta.Value);

        var movimientos = await query
            .OrderByDescending(m => m.RegistradoEn)
            .Take(limit)
            .ToListAsync();

        return movimientos.Select(m => new MovimientoInsumoDto
        {
            Id = m.Id,
            InsumoId = m.InsumoId,
            InsumoNombre = m.Insumo?.Nombre ?? "",
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            CostoPorUnidad = m.CostoPorUnidad,
            Motivo = m.Motivo,
            UsuarioId = m.UsuarioId,
            OrdenId = m.OrdenId,
            RegistradoEn = m.RegistradoEn
        }).ToList();
    }
}
