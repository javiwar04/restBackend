namespace WebApi.DTOs.Menu;

public class CategoriaMenuDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activa { get; set; }
}

public class CreateCategoriaMenuDto
{
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;
}

public class UpdateCategoriaMenuDto
{
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activa { get; set; }
}
