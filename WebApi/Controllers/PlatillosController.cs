using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Menu;
using WebApi.Extensions;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("platillos")]
public class PlatillosController : ControllerBase
{
    private readonly MenuService _menuService;

    public PlatillosController(MenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlatillos(
        [FromQuery] string? categoria_id = null,
        [FromQuery] bool? disponible = null,
        [FromQuery] string? q = null,
        [FromQuery] string? establecimiento = null)
    {
        // Filtro explícito (admin) > sucursal activa del header (POS)
        var estId = !string.IsNullOrWhiteSpace(establecimiento) ? establecimiento : HttpContext.GetEstablecimiento();
        var platillos = await _menuService.GetPlatillosAsync(categoria_id, disponible, q, estId);
        return Ok(platillos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlatillo(string id)
    {
        var platillo = await _menuService.GetPlatilloByIdAsync(id);

        if (platillo == null)
            return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

        return Ok(platillo);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlatillo([FromBody] CreatePlatilloDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        if (dto.Precio <= 0)
            return BadRequest(new ErrorResponse { Error = "El precio debe ser mayor a 0" });

        try
        {
            var platillo = await _menuService.CreatePlatilloAsync(dto);
            return CreatedAtAction(nameof(GetPlatillo), new { id = platillo.Id }, platillo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlatillo(string id, [FromBody] UpdatePlatilloDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        if (dto.Precio <= 0)
            return BadRequest(new ErrorResponse { Error = "El precio debe ser mayor a 0" });

        try
        {
            var platillo = await _menuService.UpdatePlatilloAsync(id, dto);

            if (platillo == null)
                return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

            return Ok(platillo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPatch("{id}/disponible")]
    public async Task<IActionResult> UpdateDisponible(string id, [FromBody] UpdateDisponibleDto dto)
    {
        var updated = await _menuService.UpdateDisponibleAsync(id, dto.Disponible);

        if (!updated)
            return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

        return Ok(new { ok = true });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlatillo(string id)
    {
        var deleted = await _menuService.DeletePlatilloAsync(id);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

        return Ok(new { ok = true });
    }
}
