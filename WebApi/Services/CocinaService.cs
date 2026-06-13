using AccesoDatos.Context;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Ordenes;

namespace WebApi.Services;

public class CocinaService
{
    private readonly RestauranteDbContext _context;
    private readonly RealtimeNotifier _realtime;

    public CocinaService(RestauranteDbContext context, RealtimeNotifier realtime)
    {
        _context = context;
        _realtime = realtime;
    }

    public async Task<List<OrdenDto>> GetOrdenesEnCocinaAsync()
    {
        var ordenes = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .ThenInclude(i => i.OrdenItemModificadores)
            .Include(o => o.OrdenItems)
            .ThenInclude(i => i.Platillo)
            .ThenInclude(p => p!.Categoria)
            .Where(o => o.Estado == "pendiente" || o.Estado == "en_cocina")
            .OrderBy(o => o.CreadoEn)
            .ToListAsync();

        return ordenes.Select(o => new OrdenDto
        {
            Id = o.Id,
            MesaId = o.MesaId,
            NumeroMesa = o.NumeroMesa,
            TipoServicio = o.TipoServicio,
            Estado = o.Estado,
            Comensales = o.Comensales,
            UsuarioNombre = o.UsuarioNombre,
            MeseroNombre = o.MeseroNombre,
            Descuento = o.Descuento,
            Propina = o.Propina,
            Subtotal = o.Subtotal,
            Impuestos = o.Impuestos,
            Total = o.Total,
            CreadoEn = o.CreadoEn,
            ActualizadoEn = o.ActualizadoEn,
            Notas = o.Notas,
            Items = o.OrdenItems.Select(i => new OrdenItemDto
            {
                Id = i.Id,
                PlatilloId = i.PlatilloId ?? "",
                Nombre = i.Nombre,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Notas = i.Notas,
                Estado = i.Estado,
                Categoria = i.Platillo != null ? i.Platillo.Categoria.Nombre : null,
                Modificadores = i.OrdenItemModificadores.Select(m => new ModificadorItemDto
                {
                    GrupoNombre = m.GrupoNombre,
                    OpcionNombre = m.OpcionNombre,
                    PrecioDelta = m.PrecioDelta
                }).ToList()
            }).ToList()
        }).ToList();
    }

    public async Task<bool> IniciarOrdenAsync(string ordenId)
    {
        var orden = await _context.Ordenes.FindAsync(ordenId);

        if (orden == null)
            return false;

        orden.Estado = "en_cocina";
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return true;
    }

    public async Task<bool> OrdenListaAsync(string ordenId)
    {
        var orden = await _context.Ordenes.FindAsync(ordenId);

        if (orden == null)
            return false;

        orden.Estado = "servido";
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("lista", ordenId, orden.NumeroMesa);

        return true;
    }

    /// <summary>
    /// Regresa una orden a "pendiente" (boton de reinicio en el monitor).
    /// </summary>
    public async Task<bool> ReiniciarOrdenAsync(string ordenId)
    {
        var orden = await _context.Ordenes.FindAsync(ordenId);

        if (orden == null)
            return false;

        orden.Estado = "pendiente";
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realtime.OrdenCambioAsync("actualizada", ordenId);

        return true;
    }

    /// <summary>
    /// Estado de un platillo individual (KDS por item). Persistente: antes el
    /// check de items era solo local y cada refresco lo borraba.
    /// </summary>
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
}
