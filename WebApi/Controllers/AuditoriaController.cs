using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Auditoria;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("auditoria")]
public class AuditoriaController : ControllerBase
{
    private readonly AuditoriaService _auditoriaService;

    public AuditoriaController(AuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

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
}
