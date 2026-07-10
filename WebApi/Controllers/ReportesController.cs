using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Reportes;
using WebApi.Extensions;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize(Roles = "admin,reports")]
[ApiController]
[Route("reportes")]
public class ReportesController : ControllerBase
{
    private readonly ReportesService _reportesService;

    public ReportesController(ReportesService reportesService)
    {
        _reportesService = reportesService;
    }

    // Sucursal para filtrar: query explícito (admin) o header. Sin ninguno = consolidado.
    private string? EstId(string? establecimiento)
        => !string.IsNullOrWhiteSpace(establecimiento) ? establecimiento : HttpContext.GetEstablecimiento();

    [HttpGet("ventas")]
    public async Task<IActionResult> GetReporteVentas(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? establecimiento = null)
    {
        var hoy = DateTime.UtcNow.Date;
        var fechaDesde = desde ?? hoy;
        var fechaHasta = hasta ?? hoy.AddDays(1).AddSeconds(-1);

        var reporte = await _reportesService.GetReporteVentasAsync(fechaDesde, fechaHasta, EstId(establecimiento));
        return Ok(reporte);
    }

    [HttpGet("platillos")]
    public async Task<IActionResult> GetReportePlatillos(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? establecimiento = null)
    {
        var hoy = DateTime.UtcNow.Date;
        var fechaDesde = desde ?? hoy;
        var fechaHasta = hasta ?? hoy.AddDays(1).AddSeconds(-1);

        var reporte = await _reportesService.GetReportePlatillosAsync(fechaDesde, fechaHasta, EstId(establecimiento));
        return Ok(reporte);
    }

    [HttpGet("corte-caja")]
    public async Task<IActionResult> GetReporteCorteCaja([FromQuery] string? turnoId = null, [FromQuery] string? establecimiento = null)
    {
        var reporte = await _reportesService.GetReporteCorteCajaAsync(turnoId, EstId(establecimiento));

        if (reporte == null)
            return NotFound(new { error = "No se encontr� turno" });

        return Ok(reporte);
    }

    [HttpGet("meseros")]
    public async Task<IActionResult> GetReporteMeseros(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? establecimiento = null)
    {
        var hoy = DateTime.UtcNow.Date;
        var fechaDesde = desde ?? hoy;
        var fechaHasta = hasta ?? hoy.AddDays(1).AddSeconds(-1);

        var reporte = await _reportesService.GetReporteMeserosAsync(fechaDesde, fechaHasta, EstId(establecimiento));
        return Ok(reporte);
    }
}
