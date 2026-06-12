using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class MetodosPago
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public bool Activo { get; set; }

    public bool RequiereReferencia { get; set; }
}
