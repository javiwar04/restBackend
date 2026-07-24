namespace WebApi.DTOs.Menu;

public class PlatilloDto
{
    public string Id { get; set; } = null!;
    public string CategoriaId { get; set; } = null!;
    public string CategoriaNombre { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Disponible { get; set; }
    public string? ImagenUrl { get; set; }
    // Sucursales donde se ofrece el platillo
    public List<string> Establecimientos { get; set; } = new();
    public List<ModificadorGrupoDto> Modificadores { get; set; } = new();
}

public class ModificadorGrupoDto
{
    public string GrupoId { get; set; } = null!;
    public string GrupoNombre { get; set; } = null!;
    public string Tipo { get; set; } = null!; // "single" | "multiple"
    public bool Obligatorio { get; set; }
    public int MinSelecciones { get; set; }
    public int MaxSelecciones { get; set; }
    public int Orden { get; set; }
    public List<ModificadorOpcionDto> Opciones { get; set; } = new();
}

public class ModificadorOpcionDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal PrecioDelta { get; set; }
    public string? InsumoId { get; set; }
    public string? InsumoNombre { get; set; }
    public decimal? CantidadInsumo { get; set; }
    public bool EsDefault { get; set; }
    public bool Activo { get; set; }
    public int Orden { get; set; }
}

public class CreatePlatilloDto
{
    public string CategoriaId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Disponible { get; set; } = true;
    public string? ImagenUrl { get; set; }
    // Sucursales donde se ofrece; null = no tocar, [] = ninguna, [...] = esas
    public List<string>? EstablecimientoIds { get; set; }
    public List<CreateModificadorGrupoDto>? Modificadores { get; set; }
}

public class UpdatePlatilloDto
{
    public string CategoriaId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Disponible { get; set; }
    public string? ImagenUrl { get; set; }
    public List<string>? EstablecimientoIds { get; set; }
    public List<CreateModificadorGrupoDto>? Modificadores { get; set; }
}

public class CreateModificadorGrupoDto
{
    public string GrupoNombre { get; set; } = null!;
    public string Tipo { get; set; } = "single";
    public bool Obligatorio { get; set; }
    public int MinSelecciones { get; set; }
    public int MaxSelecciones { get; set; }
    public int Orden { get; set; }
    public List<CreateModificadorOpcionDto> Opciones { get; set; } = new();
}

public class CreateModificadorOpcionDto
{
    public string Nombre { get; set; } = null!;
    public decimal PrecioDelta { get; set; }
    public string? InsumoId { get; set; }
    public decimal? CantidadInsumo { get; set; }
    public bool EsDefault { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}

public class UpdateDisponibleDto
{
    public bool Disponible { get; set; }
}
