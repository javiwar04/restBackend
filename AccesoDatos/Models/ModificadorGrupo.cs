namespace AccesoDatos.Models;

public class ModificadorGrupo
{
    public string Id { get; set; } = null!;
    public string PlatilloId { get; set; } = null!;
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "single"; // "single" | "multiple"
    public bool Obligatorio { get; set; }
    public int MinSelecciones { get; set; }
    public int MaxSelecciones { get; set; }
    public int Orden { get; set; }

    // Navigation
    public virtual Platillo Platillo { get; set; } = null!;
    public virtual ICollection<ModificadorOpcion> Opciones { get; set; } = new List<ModificadorOpcion>();
}
