namespace WebApi.DTOs.Config;

public class ConfigNegocioDto
{
    public string Nombre { get; set; } = null!;
    public string? SucursalNombre { get; set; }
    public string? Rfc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Logo { get; set; }
    public string Moneda { get; set; } = "MXN";
    public string ZonaHoraria { get; set; } = "America/Mexico_City";
    public string? TicketHeader { get; set; }
    public string? TicketFooter { get; set; }
}

public class ConfigImpuestosDto
{
    public bool IvaActivo { get; set; }
    public decimal IvaPorcentaje { get; set; }
    public decimal IepsTabaco { get; set; }
    public decimal IepsBebidas { get; set; }
    public bool PreciosConIva { get; set; }
    public bool PropinaActiva { get; set; }
    public decimal PropinaSugerida { get; set; }
    public bool CargoServicioActivo { get; set; }
    public decimal CargoServicioPorcentaje { get; set; }
}

public class MetodoPagoDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public bool Activo { get; set; }
    public bool RequiereReferencia { get; set; }
}

public class CreateMetodoPagoDto
{
    public string Nombre { get; set; } = null!;
    // Opcional: si no viene, el servicio lo genera del nombre (slug)
    public string? Codigo { get; set; }
    public bool RequiereReferencia { get; set; }
}

public class UpdateMetodoPagoDto
{
    public string? Nombre { get; set; }
    public bool? Activo { get; set; }
    public bool? RequiereReferencia { get; set; }
}

public class VerificarPinDto
{
    public string Pin { get; set; } = null!;
}

public class VerificarPinResponse
{
    public bool Ok { get; set; }
    public UsuarioVerificadoDto Usuario { get; set; } = null!;
}

public class UsuarioVerificadoDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Rol { get; set; } = null!;
}

public class ComandaAdminDto
{
    public string Id { get; set; } = null!;
    public string? MesaId { get; set; }
    public int? NumeroMesa { get; set; }
    public string TipoServicio { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public int? Comensales { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? MeseroNombre { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }
    public string? Notas { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public List<ComandaItemAdminDto> Items { get; set; } = new();
}

public class ComandaItemAdminDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Notas { get; set; }
    public string Estado { get; set; } = null!;
}

public class EditarComandaDto
{
    public decimal? Descuento { get; set; }
    public decimal? Propina { get; set; }
    public string? Notas { get; set; }
}

public class AnularComandaDto
{
    public string Motivo { get; set; } = null!;
}

public class TicketReimpresionDto
{
    public string OrdenId { get; set; } = null!;
    public string NegocioNombre { get; set; } = null!;
    public string? SucursalNombre { get; set; }
    public string? NegocioDireccion { get; set; }
    public string? NegocioTelefono { get; set; }
    public string? TicketHeader { get; set; }
    public string? TicketFooter { get; set; }
    public int? NumeroMesa { get; set; }
    public string? MeseroNombre { get; set; }
    public DateTime FechaHora { get; set; }
    public List<ComandaItemAdminDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }
}
