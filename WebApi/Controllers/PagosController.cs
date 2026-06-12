using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Pagos;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("pagos")]
public class PagosController : ControllerBase
{
    private readonly PagosService _pagosService;

    public PagosController(PagosService pagosService)
    {
        _pagosService = pagosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagos(
        [FromQuery] string? turno_id = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] bool? facturado = null,
        [FromQuery] int limit = 100)
    {
        var pagos = await _pagosService.GetPagosAsync(turno_id, desde, hasta, facturado, limit);
        return Ok(pagos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPago(string id)
    {
        var pago = await _pagosService.GetPagoByIdAsync(id);

        if (pago == null)
            return NotFound(new ErrorResponse { Error = "Pago no encontrado" });

        return Ok(pago);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePago([FromBody] CreatePagoDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(usuarioNombre))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        if (!dto.Tenders.Any())
            return BadRequest(new ErrorResponse { Error = "Debe haber al menos un método de pago" });

        if (dto.Tenders.Any(t => t.Monto <= 0))
            return BadRequest(new ErrorResponse { Error = "Los montos deben ser mayores a 0" });

        try
        {
            var pago = await _pagosService.CreatePagoAsync(dto, usuarioId, usuarioNombre);
            return CreatedAtAction(nameof(GetPago), new { id = pago.Id }, pago);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPatch("{id}/facturado")]
    public async Task<IActionResult> UpdateFacturado(string id, [FromBody] UpdateFacturadoDto dto)
    {
        var updated = await _pagosService.UpdateFacturadoAsync(id, dto.Facturado);

        if (!updated)
            return NotFound(new ErrorResponse { Error = "Pago no encontrado" });

        return Ok(new { ok = true });
    }
}
