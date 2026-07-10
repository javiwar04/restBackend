using AccesoDatos.Context;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Reportes;

namespace WebApi.Services;

public class ReportesService
{
    private readonly RestauranteDbContext _context;

    public ReportesService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<VentasReportDto> GetReporteVentasAsync(DateTime desde, DateTime hasta, string? establecimientoId = null)
    {
        var query = _context.Pagos
            .Include(p => p.PagosDetalles)
            .Include(p => p.Orden)
            .Where(p => p.RegistradoEn >= desde && p.RegistradoEn <= hasta);

        // Filtro por sucursal (vía la orden); sin él = consolidado del negocio
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(p => p.Orden.EstablecimientoId == establecimientoId);

        var pagos = await query.ToListAsync();

        var totalVentas = pagos.Sum(p => p.MontoTotal);
        var totalOrdenes = pagos.Count;
        var ticketPromedio = totalOrdenes > 0 ? totalVentas / totalOrdenes : 0;

        var porMetodoPago = pagos
            .SelectMany(p => p.PagosDetalles)
            .GroupBy(pd => PagosService.NormalizarMetodo(pd.MetodoCodigo))
            .ToDictionary(g => g.Key, g => g.Sum(pd => pd.Monto));

        var porDia = pagos
            .GroupBy(p => p.RegistradoEn.Date)
            .Select(g => new VentaPorDiaDto
            {
                Fecha = g.Key.ToString("yyyy-MM-dd"),
                Total = g.Sum(p => p.MontoTotal),
                Ordenes = g.Count()
            })
            .OrderBy(v => v.Fecha)
            .ToList();

        // Desglose por sucursal (nombres desde Establecimientos)
        var nombres = await _context.Establecimientos.ToDictionaryAsync(e => e.Id, e => e.Nombre);
        var porEstablecimiento = pagos
            .GroupBy(p => p.Orden.EstablecimientoId)
            .Select(g => new VentaPorEstablecimientoDto
            {
                EstablecimientoId = g.Key,
                Nombre = g.Key != null && nombres.TryGetValue(g.Key, out var n) ? n : "Sin sucursal",
                Total = g.Sum(p => p.MontoTotal),
                Ordenes = g.Count()
            })
            .OrderByDescending(e => e.Total)
            .ToList();

        return new VentasReportDto
        {
            Desde = desde.ToString("yyyy-MM-dd"),
            Hasta = hasta.ToString("yyyy-MM-dd"),
            TotalVentas = totalVentas,
            TotalOrdenes = totalOrdenes,
            TicketPromedio = ticketPromedio,
            PorMetodoPago = porMetodoPago,
            PorDia = porDia,
            PorEstablecimiento = porEstablecimiento
        };
    }

    public async Task<PlatillosReportDto> GetReportePlatillosAsync(DateTime desde, DateTime hasta, string? establecimientoId = null)
    {
        var items = await _context.OrdenItems
            .Include(i => i.Orden)
            .Where(i => i.Orden.Estado == "pagado" &&
                        i.Orden.CreadoEn >= desde &&
                        i.Orden.CreadoEn <= hasta &&
                        (establecimientoId == null || i.Orden.EstablecimientoId == establecimientoId))
            .GroupBy(i => new { i.PlatilloId, i.Nombre })
            .Select(g => new
            {
                PlatilloId = g.Key.PlatilloId ?? "",
                Nombre = g.Key.Nombre,
                CantidadVendida = g.Sum(i => (int)i.Cantidad),
                TotalGenerado = g.Sum(i => i.PrecioUnitario * i.Cantidad)
            })
            .ToListAsync();

        var totalGeneral = items.Sum(i => i.TotalGenerado);

        var platillos = items.Select(i => new PlatilloVendidoDto
        {
            PlatilloId = i.PlatilloId,
            Nombre = i.Nombre,
            CantidadVendida = i.CantidadVendida,
            TotalGenerado = i.TotalGenerado,
            PorcentajeSobreTotal = totalGeneral > 0 ? (i.TotalGenerado / totalGeneral) * 100 : 0
        })
        .OrderByDescending(p => p.TotalGenerado)
        .ToList();

        return new PlatillosReportDto
        {
            Desde = desde.ToString("yyyy-MM-dd"),
            Hasta = hasta.ToString("yyyy-MM-dd"),
            Platillos = platillos
        };
    }

    public async Task<CorteCajaReportDto?> GetReporteCorteCajaAsync(string? turnoId, string? establecimientoId = null)
    {
        AccesoDatos.Models.Turno? turno;

        if (!string.IsNullOrEmpty(turnoId))
        {
            turno = await _context.Turnos.FindAsync(turnoId);
        }
        else
        {
            // El último turno (de la sucursal si se indica)
            turno = await _context.Turnos
                .Where(t => establecimientoId == null || t.EstablecimientoId == establecimientoId)
                .OrderByDescending(t => t.Inicio)
                .FirstOrDefaultAsync();
        }

        if (turno == null)
            return null;

        var corte = await _context.CorteCajas
            .FirstOrDefaultAsync(c => c.TurnoId == turno.Id);

        var porMetodoPago = new Dictionary<string, decimal>
        {
            ["efectivo"] = turno.VentasEfectivo,
            ["tarjeta"] = turno.VentasTarjeta,
            ["transferencia"] = turno.VentasTransfer
        };

        return new CorteCajaReportDto
        {
            TurnoId = turno.Id,
            UsuarioNombre = turno.UsuarioNombre,
            IniciadoEn = turno.Inicio,
            CerradoEn = turno.Fin,
            EfectivoInicial = corte?.EfectivoInicial ?? 0,
            EfectivoFinalSistema = corte?.EfectivoFinalSistema ?? 0,
            EfectivoFinalReal = corte?.EfectivoFinalReal ?? 0,
            Diferencia = corte?.Diferencia ?? 0,
            TotalOrdenes = turno.TotalOrdenes,
            TotalVentas = turno.TotalVentas,
            PorMetodoPago = porMetodoPago
        };
    }

    public async Task<MeserosReportDto> GetReporteMeserosAsync(DateTime desde, DateTime hasta, string? establecimientoId = null)
    {
        var ordenes = await _context.Ordenes
            .Where(o => o.Estado == "pagado" &&
                        o.CreadoEn >= desde &&
                        o.CreadoEn <= hasta &&
                        o.MeseroId != null &&
                        (establecimientoId == null || o.EstablecimientoId == establecimientoId))
            .GroupBy(o => new { o.MeseroId, o.MeseroNombre })
            .Select(g => new MeseroVentasDto
            {
                UsuarioId = g.Key.MeseroId ?? "",
                Nombre = g.Key.MeseroNombre ?? "",
                Ordenes = g.Count(),
                TotalVentas = g.Sum(o => o.Total)
            })
            .OrderByDescending(m => m.TotalVentas)
            .ToListAsync();

        return new MeserosReportDto
        {
            Desde = desde.ToString("yyyy-MM-dd"),
            Hasta = hasta.ToString("yyyy-MM-dd"),
            Meseros = ordenes
        };
    }
}
