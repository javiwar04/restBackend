using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Facturas;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin,billing,caja,cajero")]
[ApiController]
[Route("facturas")]
public class FacturasController : ControllerBase
{
    private readonly FacturasService _facturasService;

    public FacturasController(FacturasService facturasService)
    {
        _facturasService = facturasService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFacturas(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        var facturas = await _facturasService.GetFacturasAsync(desde, hasta, pagina, porPagina);
        return Ok(facturas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFactura(string id)
    {
        var factura = await _facturasService.GetFacturaByIdAsync(id);

        if (factura == null)
            return NotFound(new ErrorResponse { Error = "Factura no encontrada" });

        return Ok(factura);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFactura([FromBody] CreateFacturaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClienteNombre))
            return BadRequest(new ErrorResponse { Error = "El nombre del cliente es requerido" });

        if (string.IsNullOrWhiteSpace(dto.ClienteRfc))
            return BadRequest(new ErrorResponse { Error = "El RFC del cliente es requerido" });

        try
        {
            var factura = await _facturasService.CreateFacturaAsync(dto);
            return CreatedAtAction(nameof(GetFactura), new { id = factura.Id }, factura);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> CancelarFactura(string id, [FromBody] CancelarFacturaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new ErrorResponse { Error = "El motivo es requerido" });

        try
        {
            var factura = await _facturasService.CancelarFacturaAsync(id, dto.Motivo);

            if (factura == null)
                return NotFound(new ErrorResponse { Error = "Factura no encontrada" });

            return Ok(factura);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
