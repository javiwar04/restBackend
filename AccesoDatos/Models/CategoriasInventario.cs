using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class CategoriasInventario
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool Activa { get; set; }

    public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
}
