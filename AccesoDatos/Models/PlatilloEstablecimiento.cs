namespace AccesoDatos.Models;

/// <summary>
/// Puente: en qué establecimientos está disponible un platillo. Un producto
/// puede ofrecerse en varias sucursales.
/// </summary>
public partial class PlatilloEstablecimiento
{
    public string PlatilloId { get; set; } = null!;

    public string EstablecimientoId { get; set; } = null!;
}
