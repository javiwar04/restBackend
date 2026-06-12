using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class PagosDividido
{
    public long Id { get; set; }

    public string PagoId { get; set; } = null!;

    public byte PersonaNum { get; set; }

    public decimal Monto { get; set; }

    public string MetodoCodigo { get; set; } = null!;

    public bool Cobrado { get; set; }

    public DateTime? CobradoEn { get; set; }

    public virtual Pago Pago { get; set; } = null!;
}
