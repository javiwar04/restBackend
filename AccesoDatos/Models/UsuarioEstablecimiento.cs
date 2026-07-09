namespace AccesoDatos.Models;

/// <summary>
/// Puente: a qué establecimientos puede acceder un usuario (para el selector
/// de sucursal al iniciar sesión en el POS). El admin ve todos.
/// </summary>
public partial class UsuarioEstablecimiento
{
    public string UsuarioId { get; set; } = null!;

    public string EstablecimientoId { get; set; } = null!;
}
