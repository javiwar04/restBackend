using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class VwVentasMesero
{
    public string? MeseroId { get; set; }

    public string? MeseroNombre { get; set; }

    public int? Ordenes { get; set; }

    public decimal? TotalVentas { get; set; }

    public decimal? Propinas { get; set; }
}
