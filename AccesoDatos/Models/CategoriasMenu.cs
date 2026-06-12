using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class CategoriasMenu
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public bool Activa { get; set; }

    public virtual ICollection<Platillo> Platillos { get; set; } = new List<Platillo>();

    public virtual ICollection<ModificadoresGrupo> Grupos { get; set; } = new List<ModificadoresGrupo>();
}
