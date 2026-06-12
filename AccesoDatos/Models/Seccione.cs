using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Seccione
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public bool Activa { get; set; }

    public virtual ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}
