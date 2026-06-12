using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class VwMesero
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Username { get; set; } = null!;

    public bool Activo { get; set; }
}
