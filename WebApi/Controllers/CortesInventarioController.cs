using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Inventario;
using WebApi.Extensions;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("cortes-inventario")]
public class CortesInventarioController : ControllerBase
{
    private readonly CorteInventarioService _service;

    public CortesInventarioController(CorteInventarioService service)
    {
        _service = service;
    }

    // Hoja pre-llenada para el conteo de un turno (insumos, encontré, vendido teórico)
    [HttpGet("preconteo")]
    public async Task<IActionResult> GetPreconteo([FromQuery] string turnoId)
    {
        if (string.IsNullOrWhiteSpace(turnoId))
            return BadRequest(new ErrorResponse { Error = "turnoId es requerido" });

        var pre = await _service.GetPreconteoAsync(turnoId);
        if (pre == null)
            return NotFound(new ErrorResponse { Error = "Turno no encontrado" });

        return Ok(pre);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CreateCorteInventarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TurnoId) || dto.Detalles.Count == 0)
            return BadRequest(new ErrorResponse { Error = "Turno y detalles son requeridos" });

        try
        {
            var corte = await _service.CreateCorteAsync(dto, HttpContext.GetUsuarioId());
            return Ok(corte);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCorte(string id)
    {
        var corte = await _service.GetCorteAsync(id);
        if (corte == null)
            return NotFound(new ErrorResponse { Error = "Corte no encontrado" });
        return Ok(corte);
    }
}
