using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Turnos;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("turnos")]
public class TurnosController : ControllerBase
{
    private readonly TurnosService _turnosService;

    public TurnosController(TurnosService turnosService)
    {
        _turnosService = turnosService;
    }

    [HttpGet("activo")]
    public async Task<IActionResult> GetTurnoActivo()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        var turno = await _turnosService.GetTurnoActivoAsync(usuarioId);

        if (turno == null)
            return NotFound(new ErrorResponse { Error = "Sin turno activo" });

        return Ok(turno);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTurno(string id)
    {
        var turno = await _turnosService.GetTurnoByIdAsync(id);

        if (turno == null)
            return NotFound(new ErrorResponse { Error = "Turno no encontrado" });

        return Ok(turno);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] CreateTurnoDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(usuarioNombre))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        if (dto.EfectivoInicial < 0)
            return BadRequest(new ErrorResponse { Error = "El efectivo inicial no puede ser negativo" });

        try
        {
            var turno = await _turnosService.CrearTurnoAsync(usuarioId, usuarioNombre, dto.EfectivoInicial);
            return CreatedAtAction(nameof(GetTurno), new { id = turno.Id }, turno);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpPatch("{id}/cerrar")]
    public async Task<IActionResult> CerrarTurno(string id, [FromBody] CerrarTurnoDto dto)
    {
        if (dto.EfectivoFinalReal < 0)
            return BadRequest(new ErrorResponse { Error = "El efectivo final no puede ser negativo" });

        try
        {
            var result = await _turnosService.CerrarTurnoAsync(id, dto.EfectivoFinalReal, dto.Notas, 0);

            if (result == null)
                return NotFound(new ErrorResponse { Error = "Turno no encontrado" });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
