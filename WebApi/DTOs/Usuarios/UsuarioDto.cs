namespace WebApi.DTOs.Usuarios;

public class UsuarioDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public bool Activo { get; set; }
    public List<string> Modules { get; set; } = new();
    public DateTime? CreadoEn { get; set; }
}

public class CreateUsuarioDto
{
    public string Nombre { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Pin { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public List<string> Modules { get; set; } = new();
}

public class UpdateUsuarioDto
{
    public string? Nombre { get; set; }
    public string? Username { get; set; }
    public string? Pin { get; set; }
    public string? Rol { get; set; }
    public List<string>? Modules { get; set; }
    public bool? Activo { get; set; }
}
