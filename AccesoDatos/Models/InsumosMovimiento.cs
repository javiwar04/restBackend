using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class InsumosMovimiento
{
    public long Id { get; set; }

    public string InsumoId { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal? CostoPorUnidad { get; set; }

    public string Motivo { get; set; } = null!;

    public string? UsuarioId { get; set; }

    public string? OrdenId { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual Insumo Insumo { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
