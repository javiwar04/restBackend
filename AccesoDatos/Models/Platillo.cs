using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Platillo
{
    public string Id { get; set; } = null!;

    public string CategoriaId { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public bool Disponible { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual CategoriasMenu Categoria { get; set; } = null!;

    public virtual ICollection<OrdenItem> OrdenItems { get; set; } = new List<OrdenItem>();

    public virtual ICollection<PlatilloModificadoresOverride> PlatilloModificadoresOverrides { get; set; } = new List<PlatilloModificadoresOverride>();

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();

    public virtual ICollection<ModificadorGrupo> Modificadores { get; set; } = new List<ModificadorGrupo>();
}
