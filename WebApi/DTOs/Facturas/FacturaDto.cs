namespace WebApi.DTOs.Facturas;

public class FacturaDto
{
    // Id real (GUID). Antes era int y siempre llegaba 0 al frontend,
    // lo que hacía imposible cancelar o consultar una factura.
    public string Id { get; set; } = null!;
    public string PagoId { get; set; } = null!;
    public string Folio { get; set; } = null!;
    public string ClienteNombre { get; set; } = null!;
    public string ClienteRfc { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
    public bool Cancelada { get; set; }
}

public class FacturasListDto
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int PorPagina { get; set; }
    public List<FacturaDto> Datos { get; set; } = new();
}

public class CreateFacturaDto
{
    public string PagoId { get; set; } = null!;
    public string ClienteNombre { get; set; } = null!;
    public string ClienteRfc { get; set; } = null!;
    public string? UsoCfdi { get; set; }
}

public class CancelarFacturaDto
{
    public string Motivo { get; set; } = null!;
}
