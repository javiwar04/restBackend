using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Ordenes;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("ordenes")]
public class OrdenesController : ControllerBase
{
    private readonly OrdenesService _ordenesService;

    public OrdenesController(OrdenesService ordenesService)
    {
        _ordenesService = ordenesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrdenes(
        [FromQuery] string? estado = null,
        [FromQuery] string? mesa_id = null,
        [FromQuery] string? turno_id = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int limit = 100)
    {
        var ordenes = await _ordenesService.GetOrdenesAsync(estado, mesa_id, turno_id, desde, hasta, limit);
        return Ok(ordenes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrden(string id)
    {
        var orden = await _ordenesService.GetOrdenByIdAsync(id);

        if (orden == null)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(orden);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrden([FromBody] CreateOrdenDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(usuarioNombre))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        if (!dto.Items.Any())
            return BadRequest(new ErrorResponse { Error = "La orden debe tener al menos un item" });

        try
        {
            var orden = await _ordenesService.CreateOrdenAsync(dto, usuarioId, usuarioNombre);
            return CreatedAtAction(nameof(GetOrden), new { id = orden.Id }, orden);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrden(string id, [FromBody] UpdateOrdenDto dto)
    {
        if (dto.Descuento < 0)
            return BadRequest(new ErrorResponse { Error = "El descuento no puede ser negativo" });

        if (dto.Propina < 0)
            return BadRequest(new ErrorResponse { Error = "La propina no puede ser negativa" });

        var orden = await _ordenesService.UpdateOrdenAsync(id, dto);

        if (orden == null)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(orden);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(string id, [FromBody] UpdateEstadoDto dto)
    {
        var updated = await _ordenesService.UpdateEstadoAsync(id, dto.Estado);

        if (!updated)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(new { ok = true });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrden(string id)
    {
        var deleted = await _ordenesService.DeleteOrdenAsync(id);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(new { ok = true });
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(string id, [FromBody] AddItemDto dto)
    {
        if (dto.Cantidad <= 0)
            return BadRequest(new ErrorResponse { Error = "La cantidad debe ser mayor a 0" });

        var orden = await _ordenesService.AddItemAsync(id, dto);

        if (orden == null)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return CreatedAtAction(nameof(GetOrden), new { id = orden.Id }, orden);
    }

    [HttpPut("{id}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(string id, long itemId, [FromBody] UpdateItemDto dto)
    {
        if (dto.Cantidad <= 0)
            return BadRequest(new ErrorResponse { Error = "La cantidad debe ser mayor a 0" });

        var orden = await _ordenesService.UpdateItemAsync(id, itemId, dto);

        if (orden == null)
            return NotFound(new ErrorResponse { Error = "Item no encontrado" });

        return Ok(orden);
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(string id, long itemId)
    {
        var deleted = await _ordenesService.DeleteItemAsync(id, itemId);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Item no encontrado" });

        return Ok(new { ok = true });
    }

    [HttpPatch("{id}/items/{itemId}/estado")]
    public async Task<IActionResult> UpdateItemEstado(string id, long itemId, [FromBody] UpdateEstadoDto dto)
    {
        var updated = await _ordenesService.UpdateItemEstadoAsync(id, itemId, dto.Estado);

        if (!updated)
            return NotFound(new ErrorResponse { Error = "Item no encontrado" });

        return Ok(new { ok = true });
    }
}
