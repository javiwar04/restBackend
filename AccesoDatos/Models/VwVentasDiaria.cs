using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class VwVentasDiaria
{
    public DateOnly? Fecha { get; set; }

    public int? TotalOrdenes { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? Descuentos { get; set; }

    public decimal? Impuestos { get; set; }

    public decimal? Propinas { get; set; }

    public decimal? Total { get; set; }
}
