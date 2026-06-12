using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Turnos;

namespace WebApi.Services;

public class TurnosService
{
    private readonly RestauranteDbContext _context;

    public TurnosService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<TurnoDto?> GetTurnoActivoAsync(string usuarioId)
    {
        var turno = await _context.Turnos
            .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Fin == null);

        if (turno == null)
            return null;

        return new TurnoDto
        {
            Id = turno.Id,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            Inicio = turno.Inicio,
            Fin = turno.Fin,
            TotalVentas = turno.TotalVentas,
            TotalOrdenes = turno.TotalOrdenes,
            VentasEfectivo = turno.VentasEfectivo,
            VentasTarjeta = turno.VentasTarjeta,
            VentasTransfer = turno.VentasTransfer,
            Notas = turno.Notas
        };
    }

    public async Task<TurnoDto?> GetTurnoByIdAsync(string id)
    {
        var turno = await _context.Turnos.FindAsync(id);

        if (turno == null)
            return null;

        return new TurnoDto
        {
            Id = turno.Id,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            Inicio = turno.Inicio,
            Fin = turno.Fin,
            TotalVentas = turno.TotalVentas,
            TotalOrdenes = turno.TotalOrdenes,
            VentasEfectivo = turno.VentasEfectivo,
            VentasTarjeta = turno.VentasTarjeta,
            VentasTransfer = turno.VentasTransfer,
            Notas = turno.Notas
        };
    }

    public async Task<TurnoDto> CrearTurnoAsync(string usuarioId, string usuarioNombre, decimal efectivoInicial)
    {
        var turnoExistente = await _context.Turnos
            .AnyAsync(t => t.UsuarioId == usuarioId && t.Fin == null);

        if (turnoExistente)
            throw new InvalidOperationException("Ya existe un turno activo para este usuario");

        var turno = new Turno
        {
            Id = Guid.NewGuid().ToString(),
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            Inicio = DateTime.UtcNow,
            TotalVentas = 0,
            TotalOrdenes = 0,
            VentasEfectivo = 0,
            VentasTarjeta = 0,
            VentasTransfer = 0
        };

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();

        return new TurnoDto
        {
            Id = turno.Id,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            Inicio = turno.Inicio,
            Fin = turno.Fin,
            TotalVentas = 0,
            TotalOrdenes = 0,
            VentasEfectivo = 0,
            VentasTarjeta = 0,
            VentasTransfer = 0
        };
    }

    public async Task<TurnoConCorteDto?> CerrarTurnoAsync(string turnoId, decimal efectivoFinalReal, string? notas, decimal efectivoInicial)
    {
        var turno = await _context.Turnos.FindAsync(turnoId);

        if (turno == null)
            return null;

        if (turno.Fin != null)
            throw new InvalidOperationException("El turno ya est� cerrado");

        var pagosDetalleRaw = await _context.PagosDetalles
            .Include(pd => pd.Pago)
            .Where(pd => pd.Pago.TurnoId == turnoId)
            .GroupBy(pd => pd.MetodoCodigo)
            .Select(g => new { Metodo = g.Key, Total = g.Sum(pd => pd.Monto) })
            .ToListAsync();

        // Normalizar claves legadas en espa�ol ("efectivo" -> "cash", etc.)
        var pagosDetalle = pagosDetalleRaw
            .GroupBy(p => PagosService.NormalizarMetodo(p.Metodo))
            .Select(g => new { Metodo = g.Key, Total = g.Sum(p => p.Total) })
            .ToList();

        var totales = await _context.Pagos
            .Where(p => p.TurnoId == turnoId)
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalVentas = g.Sum(p => p.MontoTotal),
                TotalOrdenes = g.Count()
            })
            .FirstOrDefaultAsync();

        var ordenesTotales = await _context.Ordenes
            .Where(o => o.TurnoId == turnoId && o.Estado == "pagado")
            .GroupBy(o => 1)
            .Select(g => new
            {
                TotalPropinas = g.Sum(o => o.Propina),
                TotalImpuestos = g.Sum(o => o.Impuestos),
                TotalDescuentos = g.Sum(o => o.Descuento)
            })
            .FirstOrDefaultAsync();

        var ventasEfectivo = pagosDetalle.FirstOrDefault(p => p.Metodo == "cash")?.Total ?? 0;
        var ventasTarjeta = pagosDetalle.FirstOrDefault(p => p.Metodo == "card")?.Total ?? 0;
        var ventasTransfer = pagosDetalle.FirstOrDefault(p => p.Metodo == "transfer")?.Total ?? 0;

        turno.Fin = DateTime.UtcNow;
        turno.TotalVentas = totales?.TotalVentas ?? 0;
        turno.TotalOrdenes = totales?.TotalOrdenes ?? 0;
        turno.VentasEfectivo = ventasEfectivo;
        turno.VentasTarjeta = ventasTarjeta;
        turno.VentasTransfer = ventasTransfer;
        turno.Notas = notas;

        var efectivoFinalSistema = efectivoInicial + ventasEfectivo;

        var corte = new CorteCaja
        {
            Id = Guid.NewGuid().ToString(),
            TurnoId = turnoId,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            FechaInicio = turno.Inicio,
            FechaFin = turno.Fin.Value,
            EfectivoInicial = efectivoInicial,
            EfectivoFinalSistema = efectivoFinalSistema,
            EfectivoFinalReal = efectivoFinalReal,
            TotalVentas = turno.TotalVentas,
            TotalOrdenes = turno.TotalOrdenes,
            TotalEfectivo = ventasEfectivo,
            TotalTarjeta = ventasTarjeta,
            TotalTransferencia = ventasTransfer,
            TotalPropinas = ordenesTotales?.TotalPropinas ?? 0,
            TotalImpuestos = ordenesTotales?.TotalImpuestos ?? 0,
            TotalDescuentos = ordenesTotales?.TotalDescuentos ?? 0,
            Notas = notas,
            RegistradoEn = DateTime.UtcNow
        };

        _context.CorteCajas.Add(corte);
        await _context.SaveChangesAsync();

        return new TurnoConCorteDto
        {
            Turno = new TurnoDto
            {
                Id = turno.Id,
                UsuarioId = turno.UsuarioId,
                UsuarioNombre = turno.UsuarioNombre,
                Inicio = turno.Inicio,
                Fin = turno.Fin,
            TotalVentas = turno.TotalVentas,
            TotalOrdenes = turno.TotalOrdenes,
            VentasEfectivo = turno.VentasEfectivo,
            VentasTarjeta = turno.VentasTarjeta,
            VentasTransfer = turno.VentasTransfer,
            Notas = turno.Notas
            },
            Corte = new CorteDto
            {
                Id = corte.Id,
                TurnoId = corte.TurnoId,
                FechaInicio = corte.FechaInicio,
                FechaFin = corte.FechaFin,
                EfectivoInicial = corte.EfectivoInicial,
                EfectivoFinalSistema = corte.EfectivoFinalSistema,
                EfectivoFinalReal = corte.EfectivoFinalReal ?? 0,
                Diferencia = corte.Diferencia ?? 0,
                TotalVentas = corte.TotalVentas,
                TotalOrdenes = corte.TotalOrdenes,
                TotalEfectivo = corte.TotalEfectivo,
                TotalTarjeta = corte.TotalTarjeta,
                TotalTransferencia = corte.TotalTransferencia,
                TotalPropinas = corte.TotalPropinas,
                TotalImpuestos = corte.TotalImpuestos,
                TotalDescuentos = corte.TotalDescuentos,
                Notas = corte.Notas
            }
        };
    }
}
