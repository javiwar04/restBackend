namespace WebApi.DTOs.Inventario;

// Pre-conteo: lo que se muestra al abrir la hoja al cerrar turno
public class PreconteoDto
{
    public string TurnoId { get; set; } = null!;
    public string? EstablecimientoId { get; set; }
    public List<PreconteoItemDto> Items { get; set; } = new();
}

public class PreconteoItemDto
{
    public string InsumoId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal CostoUnitario { get; set; }
    public decimal Encontre { get; set; }          // sugerido: quedó del corte anterior
    public decimal Ingreso { get; set; }           // lo captura el usuario (default 0)
    public decimal Quedo { get; set; }             // sugerido: stock actual del sistema
    public decimal VendidoTeorico { get; set; }    // calculado de ventas × recetas del turno
}

public class CreateCorteInventarioDto
{
    public string TurnoId { get; set; } = null!;
    public string? Notas { get; set; }
    public List<CreateCorteDetalleDto> Detalles { get; set; } = new();
}

public class CreateCorteDetalleDto
{
    public string InsumoId { get; set; } = null!;
    public decimal Encontre { get; set; }
    public decimal Ingreso { get; set; }
    public decimal Quedo { get; set; }
}

public class CorteInventarioDto
{
    public string Id { get; set; } = null!;
    public string TurnoId { get; set; } = null!;
    public System.DateTime Fecha { get; set; }
    public decimal TotalMermaValor { get; set; }
    public List<CorteDetalleDto> Detalles { get; set; } = new();
}

public class CorteDetalleDto
{
    public string InsumoId { get; set; } = null!;
    public string InsumoNombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;
    public decimal Encontre { get; set; }
    public decimal Ingreso { get; set; }
    public decimal Quedo { get; set; }
    public decimal VendidoTeorico { get; set; }
    public decimal ConsumidoFisico { get; set; }
    public decimal Merma { get; set; }
    public decimal ValorMerma { get; set; }
}
