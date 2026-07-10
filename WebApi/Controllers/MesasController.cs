using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Mesas;
using WebApi.Extensions;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
public class MesasController : ControllerBase
{
    private readonly MesasService _mesasService;

    public MesasController(MesasService mesasService)
    {
        _mesasService = mesasService;
    }

    [HttpGet("secciones")]
    public async Task<IActionResult> GetSecciones([FromQuery] string? establecimiento = null)
    {
        // Filtro explícito (admin) > sucursal activa del header (operador)
        var estId = !string.IsNullOrWhiteSpace(establecimiento) ? establecimiento : HttpContext.GetEstablecimiento();
        var secciones = await _mesasService.GetSeccionesAsync(estId);
        return Ok(secciones);
    }

    [HttpGet("mesas")]
    public async Task<IActionResult> GetMesas(
        [FromQuery] string? seccion_id = null,
        [FromQuery] bool? activa = null,
        [FromQuery] string? establecimiento = null)
    {
        var estId = !string.IsNullOrWhiteSpace(establecimiento) ? establecimiento : HttpContext.GetEstablecimiento();
        var mesas = await _mesasService.GetMesasAsync(seccion_id, activa, estId);
        return Ok(mesas);
    }

    [HttpPost("mesas")]
    public async Task<IActionResult> CreateMesa([FromBody] CreateMesaDto dto)
    {
        if (dto.Numero <= 0)
            return BadRequest(new ErrorResponse { Error = "El n�mero de mesa debe ser mayor a 0" });

        try
        {
            var mesa = await _mesasService.CreateMesaAsync(dto);
            return CreatedAtAction(nameof(GetMesas), new { id = mesa.Id }, mesa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("mesas/{id}")]
    public async Task<IActionResult> UpdateMesa(string id, [FromBody] UpdateMesaDto dto)
    {
        if (dto.Numero <= 0)
            return BadRequest(new ErrorResponse { Error = "El n�mero de mesa debe ser mayor a 0" });

        try
        {
            var mesa = await _mesasService.UpdateMesaAsync(id, dto);

            if (mesa == null)
                return NotFound(new ErrorResponse { Error = "Mesa no encontrada" });

            return Ok(mesa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpDelete("mesas/{id}")]
    public async Task<IActionResult> DeleteMesa(string id)
    {
        var deleted = await _mesasService.DeleteMesaAsync(id);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Mesa no encontrada" });

        return Ok(new { ok = true });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("secciones")]
    public async Task<IActionResult> CreateSeccion([FromBody] CreateSeccionDto dto)
    {
        try
        {
            // Admin puede indicar la sucursal en el body; si no, la del header
            var estId = !string.IsNullOrWhiteSpace(dto.EstablecimientoId) ? dto.EstablecimientoId : HttpContext.GetEstablecimiento();
            var seccion = await _mesasService.CreateSeccionAsync(dto, estId);
            return StatusCode(201, seccion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("secciones/{id}")]
    public async Task<IActionResult> UpdateSeccion(string id, [FromBody] UpdateSeccionDto dto)
    {
        try
        {
            var seccion = await _mesasService.UpdateSeccionAsync(id, dto);

            if (seccion == null)
                return NotFound(new ErrorResponse { Error = "Secci�n no encontrada" });

            return Ok(seccion);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("secciones/{id}")]
    public async Task<IActionResult> DeleteSeccion(string id)
    {
        try
        {
            var deleted = await _mesasService.DeleteSeccionAsync(id);

            if (!deleted)
                return NotFound(new ErrorResponse { Error = "Secci�n no encontrada" });

            return Ok(new { ok = true });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }
}
