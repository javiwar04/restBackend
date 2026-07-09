using System;

namespace AccesoDatos.Models;

/// <summary>
/// Sucursal / local del negocio (ej. Tacos Michoacán → Metroplaza, Paxcamán).
/// Cada establecimiento maneja su propio inventario, caja, mesas y menú.
/// </summary>
public partial class Establecimiento
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }
}
