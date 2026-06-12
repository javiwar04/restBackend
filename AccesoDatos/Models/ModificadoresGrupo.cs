using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class ModificadoresGrupo
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual ICollection<ModificadoresOpcion> ModificadoresOpcions { get; set; } = new List<ModificadoresOpcion>();

    public virtual ICollection<PlatilloModificadoresOverride> PlatilloModificadoresOverrides { get; set; } = new List<PlatilloModificadoresOverride>();

    public virtual ICollection<CategoriasMenu> Categoria { get; set; } = new List<CategoriasMenu>();
}
