using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Ordenes;

namespace WebApi.Services;

public class OrdenesService
{
    private readonly RestauranteDbContext _context;
    private readonly RealtimeNotifier _realtime;

    public OrdenesService(RestauranteDbContext context, RealtimeNotifier realtime)
    {
        _context = context;
        _realtime = realtime;
    }

    public async Task<List<OrdenDto>> GetOrdenesAsync(
        string? estado = null,
        string? mesaId = null,
        string? turnoId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limit = 100)
    {
        var query = _context.Ordenes
            .Include(o => o.OrdenItems)
            .ThenInclude(i => i.OrdenItemModificadores)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(o => o.Estado == estado);

        if (!string.IsNullOrEmpty(mesaId))
            query = query.Where(o => o.MesaId == mesaId);

        if (!string.IsNullOrEmpty(turnoId))
            query = query.Where(o => o.TurnoId == turnoId);

        if (desde.HasValue)
            query = query.Where(o => o.CreadoEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(o => o.CreadoEn <= hasta.Value);

        var ordenes = await query
            .OrderByDescending(o => o.CreadoEn)
            .Take(limit)
            .ToListAsync();

        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<OrdenDto?> GetOrdenByIdAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .ThenInclude(i => i.OrdenItemModificadores)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return null;

        return MapToDto(orden);
    }

    public async Task<OrdenDto> CreateOrdenAsync(CreateOrdenDto dto, string usuarioId, string usuarioNombre)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();
            var ivaTasa = config?.IvaTasa ?? 0.16m;

            string? meseroNombre = null;
            int? numeroMesa = null;

            if (!string.IsNullOrEmpty(dto.MeseroId))
            {
                var mesero = await _context.Usuarios.FindAsync(dto.MeseroId);
                meseroNombre = mesero?.Nombre;
            }

            if (!string.IsNullOrEmpty(dto.MesaId))
            {
                var mesa = await _context.Mesas.FindAsync(dto.MesaId);
                numeroMesa = mesa?.Numero;
            }

            var subtotal = CalcularSubtotal(dto.Items);
            var impuestos = Math.Max(0, subtotal) * ivaTasa;
            var total = subtotal + impuestos;

            var orden = new Ordene
            {
                Id = Guid.NewGuid().ToString(),
                MesaId = dto.MesaId,
                NumeroMesa = numeroMesa,
                TipoServicio = dto.TipoServicio,
                Estado = "pendiente",
                Comensales = dto.Comensales,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                MeseroId = dto.MeseroId,
                MeseroNombre = meseroNombre,
                TurnoId = dto.TurnoId,
                Notas = dto.Notas,
                Descuento = 0,
                Propina = 0,
                Subtotal = subtotal,
                Impuestos = impuestos,
                Total = total,
                CreadoEn = DateTime.UtcNow,
                ActualizadoEn = DateTime.UtcNow
            };

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            foreach (var itemDto in dto.Items)
            {
                var item = new OrdenItem
                {
                    OrdenId = orden.Id,
                    PlatilloId = itemDto.PlatilloId,
                    Nombre = itemDto.Nombre,
                    PrecioUnitario = itemDto.PrecioUnitario,
                    Cantidad = itemDto.Cantidad,
                    Notas = itemDto.Notas,
                    Estado = "pendiente"
                };

                _context.OrdenItems.Add(item);
                await _context.SaveChangesAsync();

                foreach (var modDto in itemDto.Modificadores)
                {
                    var modificador = new OrdenItemModificadore
                    {
                        OrdenItemId = item.Id,
                        GrupoNombre = modDto.GrupoNombre,
                        OpcionNombre = modDto.OpcionNombre,
                        PrecioDelta = modDto.PrecioDelta
                    };

                    _context.OrdenItemModificadores.Add(modificador);
                }
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(dto.MesaId))
            {
                var mesa = await _context.Mesas.FindAsync(dto.MesaId);
                if (mesa != null)
                {
                    mesa.Activa = false;
                }
            }

            var alerta = new CocinaAlerta
            {
                OrdenId = orden.Id,
                Tipo = "nueva_orden",
                Vista = false,
                RegistradoEn = DateTime.UtcNow
            };

            _context.CocinaAlertas.Add(alerta);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            await _realtime.OrdenCambioAsync("nueva", orden.Id);

            return (await GetOrdenByIdAsync(orden.Id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrdenDto?> UpdateOrdenAsync(string id, UpdateOrdenDto dto)
    {
        var orden = await _context.Ordenes.FindAsync(id);

        if (orden == null)
            return null;

        var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();
        var ivaTasa = config?.IvaTasa ?? 0.16m;

        orden.Descuento = dto.Descuento;
        orden.Propina = dto.Propina;
        orden.Notas = dto.Notas;
        orden.Comensales = dto.Comensales;

        var subtotalConDescuento = Math.Max(0, orden.Subtotal - dto.Descuento);
        orden.Impuestos = subtotalConDescuento * ivaTasa;
        orden.Total = subtotalConDescuento + orden.Impuestos + dto.Propina;
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("actualizada", id);

        return await GetOrdenByIdAsync(id);
    }

    public async Task<bool> UpdateEstadoAsync(string id, string estado)
    {
        var orden = await _context.Ordenes.FindAsync(id);

        if (orden == null)
            return false;

        orden.Estado = estado;
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("actualizada", id);

        return true;
    }

    public async Task<bool> DeleteOrdenAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Mesa)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return false;

        orden.Estado = "cancelado";
        orden.ActualizadoEn = DateTime.UtcNow;

        if (orden.Mesa != null)
        {
            orden.Mesa.Activa = true;
        }

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("cancelada", id);

        return true;
    }

    public async Task<OrdenDto?> AddItemAsync(string ordenId, AddItemDto dto)
    {
        var orden = await _context.Ordenes.FindAsync(ordenId);

        if (orden == null)
            return null;

        var item = new OrdenItem
        {
            OrdenId = ordenId,
            PlatilloId = dto.PlatilloId,
            Nombre = dto.Nombre,
            PrecioUnitario = dto.PrecioUnitario,
            Cantidad = dto.Cantidad,
            Notas = dto.Notas,
            Estado = "pendiente"
        };

        _context.OrdenItems.Add(item);
        await _context.SaveChangesAsync();

        foreach (var modDto in dto.Modificadores)
        {
            var modificador = new OrdenItemModificadore
            {
                OrdenItemId = item.Id,
                GrupoNombre = modDto.GrupoNombre,
                OpcionNombre = modDto.OpcionNombre,
                PrecioDelta = modDto.PrecioDelta
            };

            _context.OrdenItemModificadores.Add(modificador);
        }

        await _context.SaveChangesAsync();
        await RecalcularTotalesAsync(ordenId);
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return await GetOrdenByIdAsync(ordenId);
    }

    public async Task<OrdenDto?> UpdateItemAsync(string ordenId, long itemId, UpdateItemDto dto)
    {
        var item = await _context.OrdenItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrdenId == ordenId);

        if (item == null)
            return null;

        item.Cantidad = dto.Cantidad;
        item.Notas = dto.Notas;

        await _context.SaveChangesAsync();
        await RecalcularTotalesAsync(ordenId);
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return await GetOrdenByIdAsync(ordenId);
    }

    public async Task<bool> DeleteItemAsync(string ordenId, long itemId)
    {
        var item = await _context.OrdenItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrdenId == ordenId);

        if (item == null)
            return false;

        _context.OrdenItems.Remove(item);
        await _context.SaveChangesAsync();
        await RecalcularTotalesAsync(ordenId);
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return true;
    }

    public async Task<bool> UpdateItemEstadoAsync(string ordenId, long itemId, string estado)
    {
        var item = await _context.OrdenItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrdenId == ordenId);

        if (item == null)
            return false;

        item.Estado = estado;
        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return true;
    }

    private async Task RecalcularTotalesAsync(string ordenId)
    {
        var orden = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .ThenInclude(i => i.OrdenItemModificadores)
            .FirstOrDefaultAsync(o => o.Id == ordenId);

        if (orden == null)
            return;

        var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();
        var ivaTasa = config?.IvaTasa ?? 0.16m;

        decimal subtotal = 0;
        foreach (var item in orden.OrdenItems)
        {
            var precioConMods = item.PrecioUnitario + item.OrdenItemModificadores.Sum(m => m.PrecioDelta);
            subtotal += precioConMods * item.Cantidad;
        }

        orden.Subtotal = subtotal;
        var subtotalConDescuento = Math.Max(0, subtotal - orden.Descuento);
        orden.Impuestos = subtotalConDescuento * ivaTasa;
        orden.Total = subtotalConDescuento + orden.Impuestos + orden.Propina;
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private decimal CalcularSubtotal(List<CreateOrdenItemDto> items)
    {
        decimal subtotal = 0;
        foreach (var item in items)
        {
            var precioConMods = item.PrecioUnitario + item.Modificadores.Sum(m => m.PrecioDelta);
            subtotal += precioConMods * item.Cantidad;
        }
        return subtotal;
    }

    private OrdenDto MapToDto(Ordene orden)
    {
        return new OrdenDto
        {
            Id = orden.Id,
            MesaId = orden.MesaId,
            NumeroMesa = orden.NumeroMesa,
            TipoServicio = orden.TipoServicio,
            Estado = orden.Estado,
            Comensales = orden.Comensales,
            UsuarioNombre = orden.UsuarioNombre,
            MeseroNombre = orden.MeseroNombre,
            Descuento = orden.Descuento,
            Propina = orden.Propina,
            Subtotal = orden.Subtotal,
            Impuestos = orden.Impuestos,
            Total = orden.Total,
            CreadoEn = orden.CreadoEn,
            ActualizadoEn = orden.ActualizadoEn,
            Notas = orden.Notas,
            Items = orden.OrdenItems.Select(i => new OrdenItemDto
            {
                Id = i.Id,
                PlatilloId = i.PlatilloId ?? "",
                Nombre = i.Nombre,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Notas = i.Notas,
                Estado = i.Estado,
                Modificadores = i.OrdenItemModificadores.Select(m => new ModificadorItemDto
                {
                    GrupoNombre = m.GrupoNombre,
                    OpcionNombre = m.OpcionNombre,
                    PrecioDelta = m.PrecioDelta
                }).ToList()
            }).ToList()
        };
    }
}
