namespace AccesoDatos.Models;

public class ModificadorOpcion
{
    public string Id { get; set; } = null!;
    public string GrupoId { get; set; } = null!;
    public string Nombre { get; set; } = "";
    public decimal PrecioDelta { get; set; }
    public bool EsDefault { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }

    // Navigation
    public virtual ModificadorGrupo Grupo { get; set; } = null!;
}
