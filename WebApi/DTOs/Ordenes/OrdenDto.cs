namespace WebApi.DTOs.Ordenes;

public class OrdenDto
{
    public string Id { get; set; } = null!;
    public string? MesaId { get; set; }
    public int? NumeroMesa { get; set; }
    public string TipoServicio { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public byte Comensales { get; set; }
    public string ClienteNombre { get; set; } = "Consumidor Final";
    public string? UsuarioNombre { get; set; }
    public string? MeseroNombre { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public string? Notas { get; set; }
    public List<OrdenItemDto> Items { get; set; } = new();
}

public class OrdenItemDto
{
    public long Id { get; set; }
    public string PlatilloId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public short Cantidad { get; set; }
    public string? Notas { get; set; }
    public string Estado { get; set; } = null!;
    // Categoria del platillo: usada por el KDS para filtrar por estacion (parrilla/bebidas/etc.)
    public string? Categoria { get; set; }
    public List<ModificadorItemDto> Modificadores { get; set; } = new();
}

public class ModificadorItemDto
{
    public string GrupoNombre { get; set; } = null!;
    public string OpcionNombre { get; set; } = null!;
    public string? OpcionId { get; set; }
    public decimal PrecioDelta { get; set; }
}

public class CreateOrdenDto
{
    public string? MesaId { get; set; }
    public string TipoServicio { get; set; } = "mesa";
    public byte Comensales { get; set; } = 1;
    public string? MeseroId { get; set; }
    public string? TurnoId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? Notas { get; set; }
    public List<CreateOrdenItemDto> Items { get; set; } = new();
}

public class CreateOrdenItemDto
{
    public string PlatilloId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public short Cantidad { get; set; } = 1;
    public string? Notas { get; set; }
    public List<CreateModificadorItemDto> Modificadores { get; set; } = new();
}

public class CreateModificadorItemDto
{
    public string GrupoNombre { get; set; } = null!;
    public string OpcionNombre { get; set; } = null!;
    public string? OpcionId { get; set; }
    public decimal PrecioDelta { get; set; }
}

public class UpdateOrdenDto
{
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public string? Notas { get; set; }
    public byte Comensales { get; set; }
    public string? ClienteNombre { get; set; }
}

public class UpdateEstadoDto
{
    public string Estado { get; set; } = null!;
}

public class AddItemDto
{
    public string PlatilloId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public short Cantidad { get; set; } = 1;
    public string? Notas { get; set; }
    public List<CreateModificadorItemDto> Modificadores { get; set; } = new();
}

public class UpdateItemDto
{
    public short Cantidad { get; set; }
    public string? Notas { get; set; }
}
