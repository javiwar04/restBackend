using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class PagosDetalle
{
    public long Id { get; set; }

    public string PagoId { get; set; } = null!;

    public string MetodoCodigo { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? ReferenciaLote { get; set; }

    public string? ReferenciaTransf { get; set; }

    public virtual Pago Pago { get; set; } = null!;
}
