using System;

namespace AccesoDatos.Models;

/// <summary>
/// Entrada o retiro de efectivo de la caja durante un turno.
/// Antes solo vivia en localStorage del POS; ahora se persiste para que el
/// corte de caja (efectivo esperado) sea correcto y sobreviva refrescos.
/// </summary>
public partial class MovimientoCaja
{
    public long Id { get; set; }

    public string TurnoId { get; set; } = null!;

    public string Tipo { get; set; } = null!;   // entrada | retiro

    public decimal Monto { get; set; }

    public string Motivo { get; set; } = null!;

    public string? UsuarioId { get; set; }

    public string? UsuarioNombre { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual Turno Turno { get; set; } = null!;
}
