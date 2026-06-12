using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Pago
{
    public string Id { get; set; } = null!;

    public string OrdenId { get; set; } = null!;

    public string? TurnoId { get; set; }

    public string? UsuarioId { get; set; }

    public string? UsuarioNombre { get; set; }

    public string? MeseroId { get; set; }

    public string? MeseroNombre { get; set; }

    public decimal MontoTotal { get; set; }

    public bool Facturado { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Ordene Orden { get; set; } = null!;

    public virtual ICollection<PagosDetalle> PagosDetalles { get; set; } = new List<PagosDetalle>();

    public virtual ICollection<PagosDividido> PagosDivididos { get; set; } = new List<PagosDividido>();

    public virtual Turno? Turno { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
