using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Ordenes;
using WebApi.Services;

namespace WebApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("cocina")]
public class CocinaController : ControllerBase
{
    private readonly CocinaService _cocinaService;

    public CocinaController(CocinaService cocinaService)
    {
        _cocinaService = cocinaService;
    }

    [HttpGet("ordenes")]
    public async Task<IActionResult> GetOrdenesEnCocina()
    {
        var ordenes = await _cocinaService.GetOrdenesEnCocinaAsync();
        return Ok(ordenes);
    }

    [HttpPatch("ordenes/{id}/iniciar")]
    public async Task<IActionResult> IniciarOrden(string id)
    {
        var iniciada = await _cocinaService.IniciarOrdenAsync(id);

        if (!iniciada)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(new { ok = true });
    }

    [HttpPatch("ordenes/{id}/listo")]
    public async Task<IActionResult> OrdenLista(string id)
    {
        var lista = await _cocinaService.OrdenListaAsync(id);

        if (!lista)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(new { ok = true });
    }

    [HttpPatch("ordenes/{id}/reiniciar")]
    public async Task<IActionResult> ReiniciarOrden(string id)
    {
        var ok = await _cocinaService.ReiniciarOrdenAsync(id);

        if (!ok)
            return NotFound(new ErrorResponse { Error = "Orden no encontrada" });

        return Ok(new { ok = true });
    }

    private static readonly string[] EstadosItemValidos = { "pendiente", "en_preparacion", "listo" };

    [HttpPatch("ordenes/{id}/items/{itemId}/estado")]
    public async Task<IActionResult> UpdateItemEstado(string id, long itemId, [FromBody] UpdateEstadoDto dto)
    {
        if (!EstadosItemValidos.Contains(dto.Estado))
            return BadRequest(new ErrorResponse { Error = "Estado de item invalido" });

        var ok = await _cocinaService.UpdateItemEstadoAsync(id, itemId, dto.Estado);

        if (!ok)
            return NotFound(new ErrorResponse { Error = "Item no encontrado" });

        return Ok(new { ok = true });
    }
}
