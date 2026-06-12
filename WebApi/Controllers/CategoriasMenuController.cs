using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Menu;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("categorias-menu")]
public class CategoriasMenuController : ControllerBase
{
    private readonly MenuService _menuService;

    public CategoriasMenuController(MenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _menuService.GetCategoriasAsync();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoria(string id)
    {
        var categoria = await _menuService.GetCategoriaByIdAsync(id);
        
        if (categoria == null)
            return NotFound(new ErrorResponse { Error = "Categoría no encontrada" });

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaMenuDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        var categoria = await _menuService.CreateCategoriaAsync(dto);
        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoria(string id, [FromBody] UpdateCategoriaMenuDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        var categoria = await _menuService.UpdateCategoriaAsync(id, dto);

        if (categoria == null)
            return NotFound(new ErrorResponse { Error = "Categoría no encontrada" });

        return Ok(categoria);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoria(string id)
    {
        try
        {
            var deleted = await _menuService.DeleteCategoriaAsync(id);

            if (!deleted)
                return NotFound(new ErrorResponse { Error = "Categoría no encontrada" });

            return Ok(new { ok = true });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }
}
