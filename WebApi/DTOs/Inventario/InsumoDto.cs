namespace WebApi.DTOs.Inventario;

public class InsumoDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Activo { get; set; }
    public DateTime? ActualizadoEn { get; set; }
}

public class CreateInsumoDto
{
    public string Nombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class UpdateInsumoDto
{
    public string? Nombre { get; set; }
    public string? Unidad { get; set; }
    public decimal? StockActual { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? CostoUnitario { get; set; }
    public bool? Activo { get; set; }
}

public class AjusteStockDto
{
    public string Tipo { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public string Motivo { get; set; } = null!;
}

public class MovimientoInsumoDto
{
    public long Id { get; set; }
    public string InsumoId { get; set; } = null!;
    public string InsumoNombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal? CostoPorUnidad { get; set; }
    public string Motivo { get; set; } = null!;
    public string? UsuarioId { get; set; }
    public string? OrdenId { get; set; }
    public DateTime RegistradoEn { get; set; }
}

public class RecetaDto
{
    public string PlatilloId { get; set; } = null!;
    public string PlatilloNombre { get; set; } = null!;
    public List<IngredienteDto> Ingredientes { get; set; } = new();
}

public class IngredienteDto
{
    public string InsumoId { get; set; } = null!;
    public string InsumoNombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal Cantidad { get; set; }
}

public class UpdateRecetaDto
{
    public List<IngredienteRecetaDto> Ingredientes { get; set; } = new();
}

public class IngredienteRecetaDto
{
    public string InsumoId { get; set; } = null!;
    public decimal Cantidad { get; set; }
}
