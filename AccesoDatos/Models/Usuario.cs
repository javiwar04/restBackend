using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Usuario
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PinHash { get; set; } = null!;

    public string RolId { get; set; } = null!;

    public bool Activo { get; set; }

    public string? Notas { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<CorteCaja> CorteCajas { get; set; } = new List<CorteCaja>();

    public virtual ICollection<InsumosMovimiento> InsumosMovimientos { get; set; } = new List<InsumosMovimiento>();

    public virtual ICollection<Ordene> OrdeneMeseros { get; set; } = new List<Ordene>();

    public virtual ICollection<Ordene> OrdeneUsuarios { get; set; } = new List<Ordene>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Role Rol { get; set; } = null!;

    public virtual ICollection<Turno> Turnos { get; set; } = new List<Turno>();

    public virtual ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
}
