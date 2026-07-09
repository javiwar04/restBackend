using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Insumo
{
    public string Id { get; set; } = null!;

    public string? CategoriaId { get; set; }

    public string? EstablecimientoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Unidad { get; set; } = null!;

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; }

    public decimal CostoPorUnidad { get; set; }

    public string? Proveedor { get; set; }

    public bool Activo { get; set; }

    public string? Notas { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual CategoriasInventario? Categoria { get; set; }

    public virtual ICollection<InsumosMovimiento> InsumosMovimientos { get; set; } = new List<InsumosMovimiento>();

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();
}
