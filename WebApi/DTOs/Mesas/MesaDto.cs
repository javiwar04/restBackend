namespace WebApi.DTOs.Mesas;

public class SeccionDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public string? EstablecimientoId { get; set; }
    public List<MesaDto> Mesas { get; set; } = new();
}

public class MesaDto
{
    public string Id { get; set; } = null!;
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public byte Capacidad { get; set; }
    public string SeccionId { get; set; } = null!;
    public string? SeccionNombre { get; set; }
    public bool Activa { get; set; }
    public string? Notas { get; set; }
}

public class CreateMesaDto
{
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public byte Capacidad { get; set; } = 4;
    public string SeccionId { get; set; } = null!;
    public bool Activa { get; set; } = true;
    public string? Notas { get; set; }
}

public class UpdateMesaDto
{
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public byte Capacidad { get; set; }
    public string SeccionId { get; set; } = null!;
    public bool Activa { get; set; }
    public string? Notas { get; set; }
}

public class CreateSeccionDto
{
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; } = 0;
    public bool Activa { get; set; } = true;
    // Opcional: el admin (sin sucursal activa) indica a qué sucursal va
    public string? EstablecimientoId { get; set; }
}

public class UpdateSeccionDto
{
    public string? Nombre { get; set; }
    public int? Orden { get; set; }
    public bool? Activa { get; set; }
}
