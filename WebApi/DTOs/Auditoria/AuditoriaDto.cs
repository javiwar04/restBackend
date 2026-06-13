namespace WebApi.DTOs.Auditoria;

public class AuditoriaDto
{
    public long Id { get; set; }
    public string UsuarioId { get; set; } = null!;
    public string UsuarioNombre { get; set; } = null!;
    public string Accion { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string? Ip { get; set; }
    public DateTime CreadoEn { get; set; }
}

public class AuditoriaListDto
{
    public int Total { get; set; }
    public List<AuditoriaDto> Datos { get; set; } = new();
}

public class CreateAuditoriaDto
{
    public string Accion { get; set; } = null!;
    public string? Descripcion { get; set; }
}
