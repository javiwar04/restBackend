using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class CorteCaja
{
    public string Id { get; set; } = null!;

    public string? TurnoId { get; set; }

    public string? UsuarioId { get; set; }

    public string? UsuarioNombre { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int TotalOrdenes { get; set; }

    public decimal TotalVentas { get; set; }

    public decimal TotalDescuentos { get; set; }

    public decimal TotalPropinas { get; set; }

    public decimal TotalImpuestos { get; set; }

    public decimal TotalEfectivo { get; set; }

    public decimal TotalTarjeta { get; set; }

    public decimal TotalTransferencia { get; set; }

    public decimal EfectivoInicial { get; set; }

    public decimal EfectivoFinalSistema { get; set; }

    public decimal? EfectivoFinalReal { get; set; }

    public decimal? Diferencia { get; set; }

    public string? Notas { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual Turno? Turno { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
