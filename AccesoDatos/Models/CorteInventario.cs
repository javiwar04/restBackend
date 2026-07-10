using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

/// <summary>
/// Conteo/corte de inventario de un turno (análogo al corte de caja pero de
/// producto). Cuadra el consumo físico contra lo vendido según recetas.
/// </summary>
public partial class CorteInventario
{
    public string Id { get; set; } = null!;

    public string TurnoId { get; set; } = null!;

    public string? EstablecimientoId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal TotalMermaValor { get; set; }

    public string? Notas { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual ICollection<CorteInventarioDetalle> Detalles { get; set; } = new List<CorteInventarioDetalle>();
}

public partial class CorteInventarioDetalle
{
    public long Id { get; set; }

    public string CorteId { get; set; } = null!;

    public string InsumoId { get; set; } = null!;

    public decimal Encontre { get; set; }

    public decimal Ingreso { get; set; }

    public decimal Quedo { get; set; }

    public decimal VendidoTeorico { get; set; }

    public decimal ConsumidoFisico { get; set; }

    public decimal Merma { get; set; }

    public decimal CostoUnitario { get; set; }

    public virtual CorteInventario Corte { get; set; } = null!;

    public virtual Insumo Insumo { get; set; } = null!;
}
