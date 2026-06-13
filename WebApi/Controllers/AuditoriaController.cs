using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Auditoria;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("auditoria")]
public class AuditoriaController : ControllerBase
{
    private readonly AuditoriaService _auditoriaService;

    public AuditoriaController(AuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    // Consultar la bitacora es solo para admin
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAuditoria(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? usuarioId = null,
        [FromQuery] string? accion = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        var auditoria = await _auditoriaService.GetAuditoriaAsync(desde, hasta, usuarioId, accion, pagina, porPagina);
        return Ok(auditoria);
    }

    // Registrar un evento: cualquier usuario autenticado (POS, caja) puede dejar
    // huella de acciones sensibles (cancelaciones, descuentos autorizados, etc.).
    // El usuario sale del token, no del body, para que no se pueda falsear.
    [HttpPost]
    public async Task<IActionResult> RegistrarAuditoria([FromBody] CreateAuditoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Accion))
            return BadRequest(new ErrorResponse { Error = "La accion es requerida" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, dto.Accion, dto.Descripcion ?? "", ip);

        return Ok(new { ok = true });
    }
}
