using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Ordene
{
    public string Id { get; set; } = null!;

    public string? EstablecimientoId { get; set; }

    public string? MesaId { get; set; }

    public int? NumeroMesa { get; set; }

    public string TipoServicio { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public byte Comensales { get; set; }

    public string ClienteNombre { get; set; } = "Consumidor Final";

    public string? UsuarioId { get; set; }

    public string? UsuarioNombre { get; set; }

    public string? MeseroId { get; set; }

    public string? MeseroNombre { get; set; }

    public string? TurnoId { get; set; }

    public decimal Descuento { get; set; }

    public decimal Propina { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Impuestos { get; set; }

    public decimal Total { get; set; }

    public string? Notas { get; set; }

    public DateTime CreadoEn { get; set; }

    public DateTime ActualizadoEn { get; set; }

    public virtual ICollection<CocinaAlerta> CocinaAlerta { get; set; } = new List<CocinaAlerta>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Mesa? Mesa { get; set; }

    public virtual Usuario? Mesero { get; set; }

    public virtual ICollection<OrdenItem> OrdenItems { get; set; } = new List<OrdenItem>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Turno? Turno { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
