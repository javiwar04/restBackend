using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class OrdenItem
{
    public long Id { get; set; }

    public string OrdenId { get; set; } = null!;

    public string? PlatilloId { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }

    public short Cantidad { get; set; }

    public string? Notas { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Ordene Orden { get; set; } = null!;

    public virtual ICollection<OrdenItemModificadore> OrdenItemModificadores { get; set; } = new List<OrdenItemModificadore>();

    public virtual Platillo? Platillo { get; set; }
}
