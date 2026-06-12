using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Factura
{
    public string Id { get; set; } = null!;

    public string PagoId { get; set; } = null!;

    public string OrdenId { get; set; } = null!;

    public string Folio { get; set; } = null!;

    public string? ClienteNombre { get; set; }

    public string? ClienteRfc { get; set; }

    public string? ClienteEmail { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Impuestos { get; set; }

    public decimal Total { get; set; }

    public string? CfdiUuid { get; set; }

    public string? CfdiXml { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime EmitidaEn { get; set; }

    public virtual Ordene Orden { get; set; } = null!;

    public virtual Pago Pago { get; set; } = null!;
}
