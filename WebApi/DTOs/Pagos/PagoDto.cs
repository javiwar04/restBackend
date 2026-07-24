namespace WebApi.DTOs.Pagos;

public class PagoDto
{
    public string Id { get; set; } = null!;
    public string OrdenId { get; set; } = null!;
    public string? EstablecimientoId { get; set; }
    public string? TurnoId { get; set; }
    public string? MeseroId { get; set; }
    public string? MeseroNombre { get; set; }
    public string? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public decimal MontoTotal { get; set; }
    public int? TicketNumero { get; set; }
    public string? TicketCorrelativo { get; set; }
    public bool Facturado { get; set; }
    public DateTime RegistradoEn { get; set; }
    public List<TenderDto> Tenders { get; set; } = new();
}

public class TenderDto
{
    public string Metodo { get; set; } = null!;
    public decimal Monto { get; set; }
    public string? ReferenciaLote { get; set; }
    public string? ReferenciaTransf { get; set; }
}

public class CreatePagoDto
{
    public string OrdenId { get; set; } = null!;
    public string? TurnoId { get; set; }
    public string? MeseroId { get; set; }
    public List<TenderDto> Tenders { get; set; } = new();
}

public class UpdateFacturadoDto
{
    public bool Facturado { get; set; }
}
