namespace WebApi.DTOs.Turnos;

public class TurnoDto
{
    public string Id { get; set; } = null!;
    public string UsuarioId { get; set; } = null!;
    public string UsuarioNombre { get; set; } = null!;
    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal VentasEfectivo { get; set; }
    public decimal VentasTarjeta { get; set; }
    public decimal VentasTransfer { get; set; }
    public string? Notas { get; set; }
}

public class CreateTurnoDto
{
    public decimal EfectivoInicial { get; set; }
}

public class CerrarTurnoDto
{
    public decimal EfectivoFinalReal { get; set; }
    public string? Notas { get; set; }
}

public class TurnoConCorteDto
{
    public TurnoDto Turno { get; set; } = null!;
    public CorteDto Corte { get; set; } = null!;
}

public class CorteDto
{
    public string Id { get; set; } = null!;
    public string TurnoId { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal EfectivoInicial { get; set; }
    public decimal EfectivoFinalSistema { get; set; }
    public decimal EfectivoFinalReal { get; set; }
    public decimal Diferencia { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public decimal TotalTransferencia { get; set; }
    public decimal TotalPropinas { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public string? Notas { get; set; }
}
