using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Modulo
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
