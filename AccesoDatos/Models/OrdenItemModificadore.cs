using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class OrdenItemModificadore
{
    public long Id { get; set; }

    public long OrdenItemId { get; set; }

    public string GrupoNombre { get; set; } = null!;

    public string OpcionNombre { get; set; } = null!;

    public decimal PrecioDelta { get; set; }

    public virtual OrdenItem OrdenItem { get; set; } = null!;
}
