using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Usuarios;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly UsuariosService _usuariosService;

    public UsuariosController(UsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _usuariosService.GetUsuariosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(string id)
    {
        var usuario = await _usuariosService.GetUsuarioByIdAsync(id);

        if (usuario == null)
            return NotFound(new ErrorResponse { Error = "Usuario no encontrado" });

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest(new ErrorResponse { Error = "El username es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Pin))
            return BadRequest(new ErrorResponse { Error = "El PIN es requerido" });

        try
        {
            var usuario = await _usuariosService.CreateUsuarioAsync(dto);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUsuario(string id, [FromBody] UpdateUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuariosService.UpdateUsuarioAsync(id, dto);

            if (usuario == null)
                return NotFound(new ErrorResponse { Error = "Usuario no encontrado" });

            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsuario(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        try
        {
            var deleted = await _usuariosService.DeleteUsuarioAsync(id, currentUserId);

            if (!deleted)
                return NotFound(new ErrorResponse { Error = "Usuario no encontrado" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
