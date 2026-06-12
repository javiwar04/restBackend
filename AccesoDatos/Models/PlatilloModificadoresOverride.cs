using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class PlatilloModificadoresOverride
{
    public string PlatilloId { get; set; } = null!;

    public string GrupoId { get; set; } = null!;

    public bool Habilitado { get; set; }

    public virtual ModificadoresGrupo Grupo { get; set; } = null!;

    public virtual Platillo Platillo { get; set; } = null!;
}
