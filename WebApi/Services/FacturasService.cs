using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Facturas;

namespace WebApi.Services;

public class FacturasService
{
    private readonly RestauranteDbContext _context;

    public FacturasService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<FacturasListDto> GetFacturasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        int pagina = 1,
        int porPagina = 50)
    {
        var query = _context.Facturas.AsQueryable();

        if (desde.HasValue)
            query = query.Where(f => f.EmitidaEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(f => f.EmitidaEn <= hasta.Value);

        var total = await query.CountAsync();

        var facturas = await query
            .OrderByDescending(f => f.EmitidaEn)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();

        return new FacturasListDto
        {
            Total = total,
            Pagina = pagina,
            PorPagina = porPagina,
            Datos = facturas.Select(f => new FacturaDto
            {
                Id = f.Id,
                PagoId = f.PagoId,
                Folio = f.Folio,
                ClienteNombre = f.ClienteNombre ?? "",
                ClienteRfc = f.ClienteRfc ?? "",
                Subtotal = f.Subtotal,
                Impuestos = f.Impuestos,
                Total = f.Total,
                FechaEmision = f.EmitidaEn,
                Cancelada = f.Estado == "cancelada"
            }).ToList()
        };
    }

    public async Task<FacturaDto?> GetFacturaByIdAsync(string id)
    {
        var factura = await _context.Facturas.FindAsync(id);

        if (factura == null)
            return null;

        return new FacturaDto
        {
            Id = factura.Id,
            PagoId = factura.PagoId,
            Folio = factura.Folio,
            ClienteNombre = factura.ClienteNombre ?? "",
            ClienteRfc = factura.ClienteRfc ?? "",
            Subtotal = factura.Subtotal,
            Impuestos = factura.Impuestos,
            Total = factura.Total,
            FechaEmision = factura.EmitidaEn,
            Cancelada = factura.Estado == "cancelada"
        };
    }

    public async Task<FacturaDto> CreateFacturaAsync(CreateFacturaDto dto)
    {
        var pago = await _context.Pagos
            .Include(p => p.Orden)
            .FirstOrDefaultAsync(p => p.Id == dto.PagoId);

        if (pago == null)
            throw new InvalidOperationException("Pago no encontrado");

        var existente = await _context.Facturas.AnyAsync(f => f.PagoId == dto.PagoId && f.Estado != "cancelada");
        if (existente)
            throw new InvalidOperationException("Ya existe una factura para este pago");

        var ultimaFactura = await _context.Facturas
            .OrderByDescending(f => f.EmitidaEn)
            .FirstOrDefaultAsync();

        var conteo = await _context.Facturas.CountAsync();
        var numeroFolio = conteo + 1;
        var folio = $"SF-{numeroFolio:D4}";

        var orden = pago.Orden;
        var subtotal = orden.Subtotal - orden.Descuento;
        var impuestos = orden.Impuestos;
        var total = subtotal + impuestos;

        var factura = new Factura
        {
            Id = Guid.NewGuid().ToString(),
            PagoId = dto.PagoId,
            OrdenId = pago.OrdenId,
            Folio = folio,
            ClienteNombre = dto.ClienteNombre,
            ClienteRfc = dto.ClienteRfc,
            Subtotal = subtotal,
            Impuestos = impuestos,
            Total = total,
            Estado = "emitida",
            EmitidaEn = DateTime.UtcNow
        };

        _context.Facturas.Add(factura);

        // El pago queda marcado como facturado en el mismo SaveChanges (atomico)
        pago.Facturado = true;

        await _context.SaveChangesAsync();

        return new FacturaDto
        {
            Id = factura.Id,
            PagoId = factura.PagoId,
            Folio = factura.Folio,
            ClienteNombre = factura.ClienteNombre ?? "",
            ClienteRfc = factura.ClienteRfc ?? "",
            Subtotal = factura.Subtotal,
            Impuestos = factura.Impuestos,
            Total = factura.Total,
            FechaEmision = factura.EmitidaEn,
            Cancelada = factura.Estado == "cancelada"
        };
    }

    public async Task<FacturaDto?> CancelarFacturaAsync(string id, string motivo)
    {
        var factura = await _context.Facturas.FindAsync(id);

        if (factura == null)
            return null;

        if (factura.Estado == "cancelada")
            throw new InvalidOperationException("La factura ya esta cancelada");

        factura.Estado = "cancelada";

        // Liberar el pago para que pueda volver a facturarse si hace falta
        var pago = await _context.Pagos.FindAsync(factura.PagoId);
        if (pago != null)
            pago.Facturado = false;

        await _context.SaveChangesAsync();

        return new FacturaDto
        {
            Id = factura.Id,
            PagoId = factura.PagoId,
            Folio = factura.Folio,
            ClienteNombre = factura.ClienteNombre ?? "",
            ClienteRfc = factura.ClienteRfc ?? "",
            Subtotal = factura.Subtotal,
            Impuestos = factura.Impuestos,
            Total = factura.Total,
            FechaEmision = factura.EmitidaEn,
            Cancelada = factura.Estado == "cancelada"
        };
    }
}
