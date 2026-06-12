using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Inventario;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin,inventory")]
[ApiController]
[Route("recetas")]
public class RecetasController : ControllerBase
{
    private readonly RecetasService _recetasService;

    public RecetasController(RecetasService recetasService)
    {
        _recetasService = recetasService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecetas()
    {
        var recetas = await _recetasService.GetRecetasAsync();
        return Ok(recetas);
    }

    [HttpGet("{platilloId}")]
    public async Task<IActionResult> GetReceta(string platilloId)
    {
        var receta = await _recetasService.GetRecetaByPlatilloIdAsync(platilloId);

        if (receta == null)
            return NotFound(new ErrorResponse { Error = "Receta no encontrada" });

        return Ok(receta);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{platilloId}")]
    public async Task<IActionResult> UpdateReceta(string platilloId, [FromBody] UpdateRecetaDto dto)
    {
        if (!dto.Ingredientes.Any())
            return BadRequest(new ErrorResponse { Error = "Debe tener al menos un ingrediente" });

        var receta = await _recetasService.UpdateRecetaAsync(platilloId, dto);

        if (receta == null)
            return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

        return Ok(receta);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{platilloId}")]
    public async Task<IActionResult> DeleteReceta(string platilloId)
    {
        var deleted = await _recetasService.DeleteRecetaAsync(platilloId);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Platillo no encontrado" });

        return NoContent();
    }
}
