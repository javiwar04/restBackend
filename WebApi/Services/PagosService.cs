using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApi.DTOs.Pagos;

namespace WebApi.Services;

public class PagosService
{
    private readonly RestauranteDbContext _context;
    private readonly RealtimeNotifier _realtime;

    public PagosService(RestauranteDbContext context, RealtimeNotifier realtime)
    {
        _context = context;
        _realtime = realtime;
    }

    /// <summary>
    /// Clave can�nica de m�todo de pago (metodos_pago.clave: cash/card/transfer).
    /// Acepta los nombres en espa�ol que enviaban versiones anteriores del POS.
    /// </summary>
    internal static string NormalizarMetodo(string metodo) => metodo switch
    {
        "efectivo" => "cash",
        "tarjeta" => "card",
        "transferencia" => "transfer",
        _ => metodo
    };

    public async Task<PagoDto> CreatePagoAsync(CreatePagoDto dto, string usuarioId, string usuarioNombre)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var orden = await _context.Ordenes
                .Include(o => o.Mesa)
                .Include(o => o.OrdenItems)
                .ThenInclude(i => i.Platillo)
                .ThenInclude(p => p.Receta)
                .ThenInclude(r => r.Insumo)
                .Include(o => o.OrdenItems)
                .ThenInclude(i => i.OrdenItemModificadores)
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenId);

            if (orden == null)
                throw new InvalidOperationException("Orden no encontrada");

            if (orden.Estado == "pagado")
                throw new InvalidOperationException("La orden ya est� pagada");

            var ticketNumero = await GetSiguienteTicketNumeroAsync(orden.EstablecimientoId);
            var ticketCorrelativo = await BuildTicketCorrelativoAsync(orden.EstablecimientoId, ticketNumero);

            var totalTenders = dto.Tenders.Sum(t => t.Monto);
            if (totalTenders < orden.Total)
                throw new InvalidOperationException("El monto total de pagos es menor al total de la orden");

            string? meseroNombre = null;
            if (!string.IsNullOrEmpty(dto.MeseroId))
            {
                var mesero = await _context.Usuarios.FindAsync(dto.MeseroId);
                meseroNombre = mesero?.Nombre;
            }

            var pago = new Pago
            {
                Id = Guid.NewGuid().ToString(),
                EstablecimientoId = orden.EstablecimientoId,
                OrdenId = dto.OrdenId,
                TurnoId = dto.TurnoId,
                MeseroId = dto.MeseroId,
                MeseroNombre = meseroNombre,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                MontoTotal = orden.Total,
                TicketNumero = ticketNumero,
                TicketCorrelativo = ticketCorrelativo,
                Facturado = false,
                RegistradoEn = DateTime.UtcNow
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            foreach (var tender in dto.Tenders)
            {
                var detalle = new PagosDetalle
                {
                    PagoId = pago.Id,
                    MetodoCodigo = NormalizarMetodo(tender.Metodo),
                    Monto = tender.Monto,
                    ReferenciaLote = tender.ReferenciaLote,
                    ReferenciaTransf = tender.ReferenciaTransf
                };

                _context.PagosDetalles.Add(detalle);
            }

            orden.Estado = "pagado";
            orden.ActualizadoEn = DateTime.UtcNow;

            if (orden.Mesa != null)
            {
                orden.Mesa.Activa = true;
            }

            // Insumos de la sucursal de la orden, indexados por nombre. La receta
            // define cantidades con un insumo cualquiera; al vender rebajamos el
            // insumo de LA SUCURSAL de la orden (mismos nombres en cada sucursal).
            var insumosSucursal = orden.EstablecimientoId == null
                ? new Dictionary<string, AccesoDatos.Models.Insumo>()
                : await _context.Insumos
                    .Where(i => i.EstablecimientoId == orden.EstablecimientoId)
                    .ToDictionaryAsync(i => i.Nombre, i => i);

            var opcionIds = orden.OrdenItems
                .SelectMany(i => i.OrdenItemModificadores)
                .Select(m => m.OpcionId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => id!)
                .Distinct()
                .ToList();

            var opcionesInventario = opcionIds.Count == 0
                ? new Dictionary<string, AccesoDatos.Models.ModificadorOpcion>()
                : await _context.ModificadorOpciones
                    .Include(o => o.Insumo)
                    .Where(o => opcionIds.Contains(o.Id) && o.InsumoId != null && o.CantidadInsumo != null && o.CantidadInsumo > 0)
                    .ToDictionaryAsync(o => o.Id, o => o);

            AccesoDatos.Models.Insumo ResolverInsumoSucursal(AccesoDatos.Models.Insumo insumo)
                => insumosSucursal.TryGetValue(insumo.Nombre, out var iSuc) ? iSuc : insumo;

            void RegistrarSalida(AccesoDatos.Models.Insumo insumo, decimal cantidadTotal, string motivo)
            {
                if (cantidadTotal <= 0) return;

                var movimiento = new InsumosMovimiento
                {
                    InsumoId = insumo.Id,
                    Tipo = "salida",
                    Cantidad = cantidadTotal,
                    CostoPorUnidad = insumo.CostoPorUnidad,
                    Motivo = motivo,
                    OrdenId = orden.Id,
                    UsuarioId = usuarioId,
                    RegistradoEn = DateTime.UtcNow
                };

                _context.InsumosMovimientos.Add(movimiento);
                insumo.StockActual -= cantidadTotal;
            }

            foreach (var item in orden.OrdenItems)
            {
                if (item.Platillo?.Receta != null && item.Platillo.Receta.Any())
                {
                    foreach (var receta in item.Platillo.Receta)
                    {
                        var cantidadTotal = receta.Cantidad * item.Cantidad;

                        // Resolver al insumo de la sucursal por nombre; si no hay, usar el de la receta
                        var insumoObjetivo = ResolverInsumoSucursal(receta.Insumo);
                        RegistrarSalida(insumoObjetivo, cantidadTotal, $"Venta - Orden {orden.Id}");
                    }
                }

                foreach (var mod in item.OrdenItemModificadores)
                {
                    if (mod.OpcionId == null || !opcionesInventario.TryGetValue(mod.OpcionId, out var opcion) || opcion.Insumo == null)
                        continue;

                    var insumoObjetivo = ResolverInsumoSucursal(opcion.Insumo);
                    var cantidadTotal = (opcion.CantidadInsumo ?? 0) * item.Cantidad;
                    RegistrarSalida(insumoObjetivo, cantidadTotal, $"Venta modificador {mod.GrupoNombre}: {mod.OpcionNombre} - Orden {orden.Id}");
                }
            }

            if (!string.IsNullOrEmpty(dto.TurnoId))
            {
                var turno = await _context.Turnos.FindAsync(dto.TurnoId);
                if (turno != null)
                {
                    turno.TotalVentas += orden.Total;
                    turno.TotalOrdenes += 1;

                    var efectivo = dto.Tenders.Where(t => NormalizarMetodo(t.Metodo) == "cash").Sum(t => t.Monto);
                    var tarjeta = dto.Tenders.Where(t => NormalizarMetodo(t.Metodo) == "card").Sum(t => t.Monto);
                    var transfer = dto.Tenders.Where(t => NormalizarMetodo(t.Metodo) == "transfer").Sum(t => t.Monto);

                    turno.VentasEfectivo += efectivo;
                    turno.VentasTarjeta += tarjeta;
                    turno.VentasTransfer += transfer;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _realtime.OrdenCambioAsync("pagada", orden.Id);

            return new PagoDto
            {
                Id = pago.Id,
                OrdenId = pago.OrdenId,
                EstablecimientoId = pago.EstablecimientoId,
                TurnoId = pago.TurnoId,
                MeseroId = pago.MeseroId,
                MeseroNombre = pago.MeseroNombre,
                UsuarioId = pago.UsuarioId,
                UsuarioNombre = pago.UsuarioNombre,
                MontoTotal = pago.MontoTotal,
                TicketNumero = pago.TicketNumero,
                TicketCorrelativo = pago.TicketCorrelativo,
                Facturado = pago.Facturado,
                RegistradoEn = pago.RegistradoEn,
                Tenders = dto.Tenders
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<PagoDto>> GetPagosAsync(
        string? turnoId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        bool? facturado = null,
        int limit = 100)
    {
        var query = _context.Pagos
            .Include(p => p.PagosDetalles)
            .AsQueryable();

        if (!string.IsNullOrEmpty(turnoId))
            query = query.Where(p => p.TurnoId == turnoId);

        if (desde.HasValue)
            query = query.Where(p => p.RegistradoEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(p => p.RegistradoEn <= hasta.Value);

        if (facturado.HasValue)
            query = query.Where(p => p.Facturado == facturado.Value);

        var pagos = await query
            .OrderByDescending(p => p.RegistradoEn)
            .Take(limit)
            .ToListAsync();

        return pagos.Select(p => new PagoDto
        {
            Id = p.Id,
            OrdenId = p.OrdenId,
            EstablecimientoId = p.EstablecimientoId,
            TurnoId = p.TurnoId,
            MeseroId = p.MeseroId,
            MeseroNombre = p.MeseroNombre,
            UsuarioId = p.UsuarioId,
            UsuarioNombre = p.UsuarioNombre,
            MontoTotal = p.MontoTotal,
            TicketNumero = p.TicketNumero,
            TicketCorrelativo = p.TicketCorrelativo,
            Facturado = p.Facturado,
            RegistradoEn = p.RegistradoEn,
            Tenders = p.PagosDetalles.Select(d => new TenderDto
            {
                Metodo = d.MetodoCodigo,
                Monto = d.Monto,
                ReferenciaLote = d.ReferenciaLote,
                ReferenciaTransf = d.ReferenciaTransf
            }).ToList()
        }).ToList();
    }

    public async Task<PagoDto?> GetPagoByIdAsync(string id)
    {
        var pago = await _context.Pagos
            .Include(p => p.PagosDetalles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pago == null)
            return null;

        return new PagoDto
        {
            Id = pago.Id,
            OrdenId = pago.OrdenId,
            EstablecimientoId = pago.EstablecimientoId,
            TurnoId = pago.TurnoId,
            MeseroId = pago.MeseroId,
            MeseroNombre = pago.MeseroNombre,
            UsuarioId = pago.UsuarioId,
            UsuarioNombre = pago.UsuarioNombre,
            MontoTotal = pago.MontoTotal,
            TicketNumero = pago.TicketNumero,
            TicketCorrelativo = pago.TicketCorrelativo,
            Facturado = pago.Facturado,
            RegistradoEn = pago.RegistradoEn,
            Tenders = pago.PagosDetalles.Select(d => new TenderDto
            {
                Metodo = d.MetodoCodigo,
                Monto = d.Monto,
                ReferenciaLote = d.ReferenciaLote,
                ReferenciaTransf = d.ReferenciaTransf
            }).ToList()
        };
    }

    public async Task<bool> UpdateFacturadoAsync(string id, bool facturado)
    {
        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null)
            return false;

        pago.Facturado = facturado;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<int> GetSiguienteTicketNumeroAsync(string? establecimientoId)
    {
        var ultimo = await _context.Pagos
            .Where(p => p.EstablecimientoId == establecimientoId && p.TicketNumero != null)
            .MaxAsync(p => (int?)p.TicketNumero) ?? 0;

        return ultimo + 1;
    }

    private async Task<string> BuildTicketCorrelativoAsync(string? establecimientoId, int numero)
    {
        var nombreSucursal = string.IsNullOrWhiteSpace(establecimientoId)
            ? null
            : await _context.Establecimientos
                .Where(e => e.Id == establecimientoId)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();

        var prefijo = BuildPrefijoSucursal(nombreSucursal);
        return $"{prefijo}-{numero:000000}";
    }

    private static string BuildPrefijoSucursal(string? nombreSucursal)
    {
        var limpio = new string((nombreSucursal ?? "TCK")
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(3)
            .ToArray());

        return string.IsNullOrWhiteSpace(limpio) ? "TCK" : limpio.PadRight(3, 'X');
    }
}
