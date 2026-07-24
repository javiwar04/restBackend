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
    public string? Notas { get; set; }
    public Dictionary<string, decimal> PorMetodoPago { get; set; } = new();
}

public class InventarioReportDto
{
    public string Desde { get; set; } = null!;
    public string Hasta { get; set; } = null!;
    public decimal TotalEntradas { get; set; }
    public decimal TotalSalidasVenta { get; set; }
    public decimal TotalMermas { get; set; }
    public decimal TotalAjustes { get; set; }
    public decimal ValorSalidasVenta { get; set; }
    public List<InsumoMovimientoResumenDto> Insumos { get; set; } = new();
    public List<NecesidadInventarioDto> Necesidades { get; set; } = new();
}

public class InsumoMovimientoResumenDto
{
    public string InsumoId { get; set; } = null!;
    public string InsumoNombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal CantidadEntrada { get; set; }
    public decimal CantidadSalidaVenta { get; set; }
    public decimal CantidadMerma { get; set; }
    public decimal CantidadAjuste { get; set; }
    public decimal ValorSalidaVenta { get; set; }
}

public class NecesidadInventarioDto
{
    public string TurnoId { get; set; } = null!;
    public string UsuarioNombre { get; set; } = null!;
    public DateTime CerradoEn { get; set; }
    public string Notas { get; set; } = null!;
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
