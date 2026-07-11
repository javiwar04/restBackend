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

    // Guatemala es UTC-6 fijo (sin horario de verano). Zona portable, sin
    // depender de la base de datos de zonas del sistema operativo.
    private static readonly TimeZoneInfo GtTz =
        TimeZoneInfo.CreateCustomTimeZone("GT", TimeSpan.FromHours(-6), "Guatemala (UTC-6)", "GT");

    // Convierte el rango recibido (interpretado como hora LOCAL de Guatemala) a
    // UTC, que es como se guardan los timestamps. Sin fechas => "hoy" en Guatemala.
    private static (DateTime desdeUtc, DateTime hastaUtc) RangoUtc(DateTime? desde, DateTime? hasta)
    {
        var ahoraGt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, GtTz);
        var d = desde ?? ahoraGt.Date;
        var h = hasta ?? ahoraGt.Date.AddDays(1).AddSeconds(-1);
        var dUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(d, DateTimeKind.Unspecified), GtTz);
        var hUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(h, DateTimeKind.Unspecified), GtTz);
        return (dUtc, hUtc);
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
        var (fechaDesde, fechaHasta) = RangoUtc(desde, hasta);
        var reporte = await _reportesService.GetReporteVentasAsync(fechaDesde, fechaHasta, EstId(establecimiento));
        return Ok(reporte);
    }

    [HttpGet("platillos")]
    public async Task<IActionResult> GetReportePlatillos(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? establecimiento = null)
    {
        var (fechaDesde, fechaHasta) = RangoUtc(desde, hasta);
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
        var (fechaDesde, fechaHasta) = RangoUtc(desde, hasta);
        var reporte = await _reportesService.GetReporteMeserosAsync(fechaDesde, fechaHasta, EstId(establecimiento));
        return Ok(reporte);
    }
}
