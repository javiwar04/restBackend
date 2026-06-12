using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class ModificadoresOpcion
{
    public string Id { get; set; } = null!;

    public string GrupoId { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal PrecioDelta { get; set; }

    public bool EsDefault { get; set; }

    public virtual ModificadoresGrupo Grupo { get; set; } = null!;
}
