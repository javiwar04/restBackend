using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Receta
{
    public string PlatilloId { get; set; } = null!;

    public string InsumoId { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public virtual Insumo Insumo { get; set; } = null!;

    public virtual Platillo Platillo { get; set; } = null!;
}
