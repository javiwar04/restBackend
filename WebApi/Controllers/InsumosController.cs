using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Inventario;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin,inventory")]
[ApiController]
[Route("insumos")]
public class InsumosController : ControllerBase
{
    private readonly InsumosService _insumosService;

    public InsumosController(InsumosService insumosService)
    {
        _insumosService = insumosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInsumos()
    {
        var insumos = await _insumosService.GetInsumosAsync();
        return Ok(insumos);
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] string? insumo_id = null,
        [FromQuery] string? tipo = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int limit = 200)
    {
        var movimientos = await _insumosService.GetMovimientosAsync(insumo_id, tipo, desde, hasta, limit);
        return Ok(movimientos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInsumo(string id)
    {
        var insumo = await _insumosService.GetInsumoByIdAsync(id);

        if (insumo == null)
            return NotFound(new ErrorResponse { Error = "Insumo no encontrado" });

        return Ok(insumo);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateInsumo([FromBody] CreateInsumoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Unidad))
            return BadRequest(new ErrorResponse { Error = "La unidad es requerida" });

        var insumo = await _insumosService.CreateInsumoAsync(dto);
        return CreatedAtAction(nameof(GetInsumo), new { id = insumo.Id }, insumo);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInsumo(string id, [FromBody] UpdateInsumoDto dto)
    {
        var insumo = await _insumosService.UpdateInsumoAsync(id, dto);

        if (insumo == null)
            return NotFound(new ErrorResponse { Error = "Insumo no encontrado" });

        return Ok(insumo);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInsumo(string id)
    {
        try
        {
            var deleted = await _insumosService.DeleteInsumoAsync(id);

            if (!deleted)
                return NotFound(new ErrorResponse { Error = "Insumo no encontrado" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPatch("{id}/ajuste")]
    public async Task<IActionResult> AjusteStock(string id, [FromBody] AjusteStockDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        if (dto.Cantidad <= 0)
            return BadRequest(new ErrorResponse { Error = "La cantidad debe ser mayor a 0" });

        if (string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new ErrorResponse { Error = "El motivo es requerido" });

        try
        {
            var insumo = await _insumosService.AjusteStockAsync(id, dto, usuarioId);

            if (insumo == null)
                return NotFound(new ErrorResponse { Error = "Insumo no encontrado" });

            return Ok(insumo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
