using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class ConfigNegocio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Rfc { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? TicketEncabezado { get; set; }

    public string? TicketPie { get; set; }

    public string Moneda { get; set; } = null!;

    public string ZonaHoraria { get; set; } = null!;
}
