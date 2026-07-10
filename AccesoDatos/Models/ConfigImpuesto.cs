using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class ConfigImpuesto
{
    public int Id { get; set; }

    public string? EstablecimientoId { get; set; }

    public bool IvaHabilitado { get; set; }

    public decimal IvaTasa { get; set; }

    public bool IvaIncluido { get; set; }

    public bool PropinaHabilitada { get; set; }

    public decimal PropinaSugerida { get; set; }

    public bool PropinaAuto { get; set; }

    public int PropinaAutoMinComensales { get; set; }

    public decimal PropinaAutoTasa { get; set; }

    public bool CargoServicioHabilitado { get; set; }

    public decimal CargoServicioTasa { get; set; }
}
