namespace WebApi.DTOs.Establecimientos;

public class EstablecimientoDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
}

public class CreateEstablecimientoDto
{
    public string Nombre { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}

public class UpdateEstablecimientoDto
{
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool? Activo { get; set; }
}
