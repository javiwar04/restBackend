using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class VwTopPlatillo
{
    public string Platillo { get; set; } = null!;

    public string? PlatilloId { get; set; }

    public int? UnidadesVendidas { get; set; }

    public decimal? IngresoTotal { get; set; }
}
