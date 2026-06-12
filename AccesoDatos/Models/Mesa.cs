using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Mesa
{
    public string Id { get; set; } = null!;

    public int Numero { get; set; }

    public string? Etiqueta { get; set; }

    public byte Capacidad { get; set; }

    public string SeccionId { get; set; } = null!;

    public bool Activa { get; set; }

    public string? Notas { get; set; }

    public virtual ICollection<Ordene> Ordenes { get; set; } = new List<Ordene>();

    public virtual Seccione Seccion { get; set; } = null!;
}
