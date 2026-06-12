using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Auditorium
{
    public long Id { get; set; }

    public string? UsuarioId { get; set; }

    public string? UsuarioNombre { get; set; }

    public string? Rol { get; set; }

    public string Accion { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
