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

    // Suma entradas/retiros del turno y arma el DTO con el efectivo en caja
    private async Task<TurnoDto> BuildTurnoDtoAsync(Turno turno)
    {
        var movs = await _context.MovimientosCaja
            .Where(m => m.TurnoId == turno.Id)
            .GroupBy(m => m.Tipo)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(x => x.Monto) })
            .ToListAsync();

        var entradas = movs.FirstOrDefault(m => m.Tipo == "entrada")?.Total ?? 0;
        var retiros = movs.FirstOrDefault(m => m.Tipo == "retiro")?.Total ?? 0;

        return new TurnoDto
        {
            Id = turno.Id,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            EstablecimientoId = turno.EstablecimientoId,
            Inicio = turno.Inicio,
            Fin = turno.Fin,
            EfectivoInicial = turno.EfectivoInicial,
            TotalVentas = turno.TotalVentas,
            TotalOrdenes = turno.TotalOrdenes,
            VentasEfectivo = turno.VentasEfectivo,
            VentasTarjeta = turno.VentasTarjeta,
            VentasTransfer = turno.VentasTransfer,
            TotalEntradas = entradas,
            TotalRetiros = retiros,
            EfectivoEnCaja = turno.EfectivoInicial + turno.VentasEfectivo + entradas - retiros,
            Notas = turno.Notas
        };
    }

    public async Task<TurnoDto?> GetTurnoActivoAsync(string usuarioId, string? establecimientoId = null)
    {
        var query = _context.Turnos.Where(t => t.UsuarioId == usuarioId && t.Fin == null);

        // La caja activa es por sucursal: cada local su turno independiente
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(t => t.EstablecimientoId == establecimientoId);

        var turno = await query.FirstOrDefaultAsync();

        return turno == null ? null : await BuildTurnoDtoAsync(turno);
    }

    public async Task<TurnoDto?> GetTurnoByIdAsync(string id)
    {
        var turno = await _context.Turnos.FindAsync(id);

        return turno == null ? null : await BuildTurnoDtoAsync(turno);
    }

    public async Task<MovimientoCajaDto?> AddMovimientoAsync(string turnoId, CreateMovimientoCajaDto dto, string? usuarioId, string? usuarioNombre)
    {
        var turno = await _context.Turnos.FindAsync(turnoId);
        if (turno == null) return null;
        if (turno.Fin != null) throw new InvalidOperationException("El turno ya está cerrado");
        if (dto.Tipo != "entrada" && dto.Tipo != "retiro") throw new InvalidOperationException("Tipo inválido (entrada/retiro)");
        if (dto.Monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a 0");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) throw new InvalidOperationException("El motivo es requerido");

        var mov = new MovimientoCaja
        {
            TurnoId = turnoId,
            Tipo = dto.Tipo,
            Monto = dto.Monto,
            Motivo = dto.Motivo.Trim(),
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            RegistradoEn = DateTime.UtcNow
        };
        _context.MovimientosCaja.Add(mov);
        await _context.SaveChangesAsync();

        return new MovimientoCajaDto
        {
            Id = mov.Id,
            TurnoId = mov.TurnoId,
            Tipo = mov.Tipo,
            Monto = mov.Monto,
            Motivo = mov.Motivo,
            UsuarioNombre = mov.UsuarioNombre,
            RegistradoEn = mov.RegistradoEn
        };
    }

    public async Task<List<MovimientoCajaDto>> GetMovimientosAsync(string turnoId)
    {
        return await _context.MovimientosCaja
            .Where(m => m.TurnoId == turnoId)
            .OrderBy(m => m.RegistradoEn)
            .Select(m => new MovimientoCajaDto
            {
                Id = m.Id,
                TurnoId = m.TurnoId,
                Tipo = m.Tipo,
                Monto = m.Monto,
                Motivo = m.Motivo,
                UsuarioNombre = m.UsuarioNombre,
                RegistradoEn = m.RegistradoEn
            })
            .ToListAsync();
    }

    public async Task<TurnoDto> CrearTurnoAsync(string usuarioId, string usuarioNombre, decimal efectivoInicial, string? establecimientoId)
    {
        // Turno activo por usuario Y sucursal (una caja abierta por local)
        var turnoExistente = await _context.Turnos
            .AnyAsync(t => t.UsuarioId == usuarioId && t.Fin == null
                && (establecimientoId == null || t.EstablecimientoId == establecimientoId));

        if (turnoExistente)
            throw new InvalidOperationException("Ya existe un turno activo para este usuario en esta sucursal");

        var turno = new Turno
        {
            Id = Guid.NewGuid().ToString(),
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            EstablecimientoId = establecimientoId,
            Inicio = DateTime.UtcNow,
            EfectivoInicial = efectivoInicial,
            TotalVentas = 0,
            TotalOrdenes = 0,
            VentasEfectivo = 0,
            VentasTarjeta = 0,
            VentasTransfer = 0
        };

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();

        return await BuildTurnoDtoAsync(turno);
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

        // Movimientos de caja del turno (entradas/retiros)
        var entradas = await _context.MovimientosCaja
            .Where(m => m.TurnoId == turnoId && m.Tipo == "entrada").SumAsync(m => (decimal?)m.Monto) ?? 0;
        var retiros = await _context.MovimientosCaja
            .Where(m => m.TurnoId == turnoId && m.Tipo == "retiro").SumAsync(m => (decimal?)m.Monto) ?? 0;

        turno.Fin = DateTime.UtcNow;
        turno.TotalVentas = totales?.TotalVentas ?? 0;
        turno.TotalOrdenes = totales?.TotalOrdenes ?? 0;
        turno.VentasEfectivo = ventasEfectivo;
        turno.VentasTarjeta = ventasTarjeta;
        turno.VentasTransfer = ventasTransfer;
        turno.Notas = notas;

        // El efectivo inicial real es el que se guardo al abrir el turno
        var efectivoInicialReal = turno.EfectivoInicial;
        var efectivoFinalSistema = efectivoInicialReal + ventasEfectivo + entradas - retiros;

        var corte = new CorteCaja
        {
            Id = Guid.NewGuid().ToString(),
            TurnoId = turnoId,
            UsuarioId = turno.UsuarioId,
            UsuarioNombre = turno.UsuarioNombre,
            FechaInicio = turno.Inicio,
            FechaFin = turno.Fin.Value,
            EfectivoInicial = efectivoInicialReal,
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
            Turno = await BuildTurnoDtoAsync(turno),
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
