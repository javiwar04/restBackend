using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Establecimientos;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("establecimientos")]
public class EstablecimientosController : ControllerBase
{
    private readonly EstablecimientosService _service;

    public EstablecimientosController(EstablecimientosService service)
    {
        _service = service;
    }

    // Los establecimientos que el usuario autenticado puede usar (selector POS)
    [HttpGet]
    public async Task<IActionResult> GetMisEstablecimientos()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var esAdmin = User.IsInRole("admin");
        var list = await _service.GetForUserAsync(usuarioId, esAdmin);
        return Ok(list);
    }

    // Lista pública (id+nombre) para el selector de la cocina, que es anónima
    [AllowAnonymous]
    [HttpGet("publicos")]
    public async Task<IActionResult> GetPublicos()
    {
        return Ok(await _service.GetAllAsync());
    }

    // Todos (para administración)
    [Authorize(Roles = "admin")]
    [HttpGet("todos")]
    public async Task<IActionResult> GetTodos()
    {
        return Ok(await _service.GetAllAsync());
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CreateEstablecimientoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        var est = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTodos), est);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, [FromBody] UpdateEstablecimientoDto dto)
    {
        var est = await _service.UpdateAsync(id, dto);
        if (est == null)
            return NotFound(new ErrorResponse { Error = "Establecimiento no encontrado" });
        return Ok(est);
    }
}
