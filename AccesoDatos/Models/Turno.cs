using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Turno
{
    public string Id { get; set; } = null!;

    public string UsuarioId { get; set; } = null!;

    public string UsuarioNombre { get; set; } = null!;

    public string? EstablecimientoId { get; set; }

    public DateTime Inicio { get; set; }

    public DateTime? Fin { get; set; }

    public decimal EfectivoInicial { get; set; }

    public decimal TotalVentas { get; set; }

    public int TotalOrdenes { get; set; }

    public decimal VentasEfectivo { get; set; }

    public decimal VentasTarjeta { get; set; }

    public decimal VentasTransfer { get; set; }

    public string? Notas { get; set; }

    public virtual ICollection<CorteCaja> CorteCajas { get; set; } = new List<CorteCaja>();

    public virtual ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();

    public virtual ICollection<Ordene> Ordenes { get; set; } = new List<Ordene>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Usuario Usuario { get; set; } = null!;
}
