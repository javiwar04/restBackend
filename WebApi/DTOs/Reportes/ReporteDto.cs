namespace WebApi.DTOs.Reportes;

public class VentasReportDto
{
    public string Desde { get; set; } = null!;
    public string Hasta { get; set; } = null!;
    public decimal TotalVentas { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal TicketPromedio { get; set; }
    public Dictionary<string, decimal> PorMetodoPago { get; set; } = new();
    public List<VentaPorDiaDto> PorDia { get; set; } = new();
    // Desglose por sucursal (cerebro financiero: aporte de cada local)
    public List<VentaPorEstablecimientoDto> PorEstablecimiento { get; set; } = new();
}

public class VentaPorEstablecimientoDto
{
    public string? EstablecimientoId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Total { get; set; }
    public int Ordenes { get; set; }
}

public class VentaPorDiaDto
{
    public string Fecha { get; set; } = null!;
    public decimal Total { get; set; }
    public int Ordenes { get; set; }
}

public class PlatillosReportDto
{
    public string Desde { get; set; } = null!;
    public string Hasta { get; set; } = null!;
    public List<PlatilloVendidoDto> Platillos { get; set; } = new();
}

public class PlatilloVendidoDto
{
    public string PlatilloId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int CantidadVendida { get; set; }
    public decimal TotalGenerado { get; set; }
    public decimal PorcentajeSobreTotal { get; set; }
}

public class CorteCajaReportDto
{
    public string TurnoId { get; set; } = null!;
    public string UsuarioNombre { get; set; } = null!;
    public DateTime IniciadoEn { get; set; }
    public DateTime? CerradoEn { get; set; }
    public decimal EfectivoInicial { get; set; }
    public decimal EfectivoFinalSistema { get; set; }
    public decimal EfectivoFinalReal { get; set; }
    public decimal Diferencia { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal TotalVentas { get; set; }
    public Dictionary<string, decimal> PorMetodoPago { get; set; } = new();
}

public class MeserosReportDto
{
    public string Desde { get; set; } = null!;
    public string Hasta { get; set; } = null!;
    public List<MeseroVentasDto> Meseros { get; set; } = new();
}

public class MeseroVentasDto
{
    public string UsuarioId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int Ordenes { get; set; }
    public decimal TotalVentas { get; set; }
}
