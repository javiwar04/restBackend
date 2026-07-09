using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Config;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("config")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService _configService;
    private readonly AuditoriaService _auditoriaService;

    public ConfigController(ConfigService configService, AuditoriaService auditoriaService)
    {
        _configService = configService;
        _auditoriaService = auditoriaService;
    }

    // ?? Negocio ????????????????????????????????????????????????????????????????

    [HttpGet("negocio")]
    public async Task<IActionResult> GetConfigNegocio()
    {
        var config = await _configService.GetConfigNegocioAsync();
        return Ok(config);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("negocio")]
    public async Task<IActionResult> UpdateConfigNegocio([FromBody] ConfigNegocioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        var config = await _configService.UpdateConfigNegocioAsync(dto);

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "config_negocio_update", "Actualiz� configuraci�n del negocio");

        return Ok(config);
    }

    // ?? Impuestos ??????????????????????????????????????????????????????????????

    [HttpGet("impuestos")]
    public async Task<IActionResult> GetConfigImpuestos()
    {
        var config = await _configService.GetConfigImpuestosAsync();
        return Ok(config);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("impuestos")]
    public async Task<IActionResult> UpdateConfigImpuestos([FromBody] ConfigImpuestosDto dto)
    {
        if (dto.IvaPorcentaje < 0 || dto.IvaPorcentaje > 100)
            return BadRequest(new ErrorResponse { Error = "El IVA debe estar entre 0 y 100" });

        if (dto.PropinaSugerida < 0 || dto.PropinaSugerida > 100)
            return BadRequest(new ErrorResponse { Error = "La propina sugerida debe estar entre 0 y 100" });

        if (dto.CargoServicioPorcentaje < 0 || dto.CargoServicioPorcentaje > 100)
            return BadRequest(new ErrorResponse { Error = "El cargo por servicio debe estar entre 0 y 100" });

        var config = await _configService.UpdateConfigImpuestosAsync(dto);

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "config_impuestos_update", "Actualiz� configuraci�n de impuestos");

        return Ok(config);
    }

    // ?? M�todos de Pago ????????????????????????????????????????????????????????

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> GetMetodosPago()
    {
        var metodos = await _configService.GetMetodosPagoAsync();
        return Ok(metodos);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("metodos-pago")]
    public async Task<IActionResult> CreateMetodoPago([FromBody] CreateMetodoPagoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ErrorResponse { Error = "El nombre es requerido" });

        // El codigo es opcional: si no viene, el servicio lo genera del nombre
        var metodo = await _configService.CreateMetodoPagoAsync(dto);

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "metodo_pago_create", $"Cre� m�todo de pago: {dto.Nombre}");

        return CreatedAtAction(nameof(GetMetodosPago), metodo);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("metodos-pago/{id}")]
    public async Task<IActionResult> UpdateMetodoPago(string id, [FromBody] UpdateMetodoPagoDto dto)
    {
        var metodo = await _configService.UpdateMetodoPagoAsync(id, dto);

        if (metodo == null)
            return NotFound(new ErrorResponse { Error = "M�todo de pago no encontrado" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "metodo_pago_update", $"Actualiz� m�todo de pago: {metodo.Nombre}");

        return Ok(metodo);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("metodos-pago/{id}")]
    public async Task<IActionResult> DeleteMetodoPago(string id)
    {
        var deleted = await _configService.DeleteMetodoPagoAsync(id);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "M�todo de pago no encontrado" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "metodo_pago_delete", $"Elimin� m�todo de pago id: {id}");

        return Ok(new { ok = true });
    }

    // ?? Comandas Admin ?????????????????????????????????????????????????????????

    [Authorize(Roles = "admin")]
    [HttpGet("comandas")]
    public async Task<IActionResult> GetComandas(
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int limit = 100)
    {
        var comandas = await _configService.GetComandasAsync(estado, desde, hasta, limit);
        return Ok(comandas);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("comandas/{id}")]
    public async Task<IActionResult> GetComanda(string id)
    {
        var comanda = await _configService.GetComandaByIdAsync(id);

        if (comanda == null)
            return NotFound(new ErrorResponse { Error = "Comanda no encontrada" });

        return Ok(comanda);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("comandas/{id}")]
    public async Task<IActionResult> EditarComanda(string id, [FromBody] EditarComandaDto dto)
    {
        if (dto.Descuento.HasValue && dto.Descuento < 0)
            return BadRequest(new ErrorResponse { Error = "El descuento no puede ser negativo" });

        if (dto.Propina.HasValue && dto.Propina < 0)
            return BadRequest(new ErrorResponse { Error = "La propina no puede ser negativa" });

        var comanda = await _configService.EditarComandaAsync(id, dto);

        if (comanda == null)
            return NotFound(new ErrorResponse { Error = "Comanda no encontrada" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "comanda_edit", $"Edit� comanda id: {id}");

        return Ok(comanda);
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("comandas/{id}/anular")]
    public async Task<IActionResult> AnularComanda(string id, [FromBody] AnularComandaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new ErrorResponse { Error = "El motivo es requerido" });

        var anulada = await _configService.AnularComandaAsync(id);

        if (!anulada)
            return NotFound(new ErrorResponse { Error = "Comanda no encontrada" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "comanda_anular", $"Anul� comanda id: {id}. Motivo: {dto.Motivo}");

        return Ok(new { ok = true });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("comandas/{id}")]
    public async Task<IActionResult> EliminarComanda(string id)
    {
        var eliminada = await _configService.EliminarComandaAsync(id);

        if (!eliminada)
            return NotFound(new ErrorResponse { Error = "Comanda no encontrada" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "comanda_delete", $"Elimin� comanda id: {id}");

        return Ok(new { ok = true });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("comandas/{id}/ticket")]
    public async Task<IActionResult> GetTicketReimpresion(string id)
    {
        var ticket = await _configService.GetTicketReimpresionAsync(id);

        if (ticket == null)
            return NotFound(new ErrorResponse { Error = "Comanda no encontrada" });

        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var usuarioNombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        await _auditoriaService.RegistrarAsync(usuarioId, usuarioNombre, "comanda_reprint", $"Reimprimi� ticket de comanda id: {id}");

        return Ok(ticket);
    }

    // ?? Verificar PIN ??????????????????????????????????????????????????????????

    [HttpPost("verificar-pin")]
    public async Task<IActionResult> VerificarPin([FromBody] VerificarPinDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Pin))
            return BadRequest(new ErrorResponse { Error = "El PIN es requerido" });

        var usuario = await _configService.VerificarPinAsync(dto.Pin);

        if (usuario == null)
            return Unauthorized(new ErrorResponse { Error = "PIN incorrecto" });

        return Ok(new VerificarPinResponse { Ok = true, Usuario = usuario });
    }
}
