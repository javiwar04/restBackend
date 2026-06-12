using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class CocinaAlerta
{
    public long Id { get; set; }

    public string OrdenId { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public bool Vista { get; set; }

    public DateTime RegistradoEn { get; set; }

    public virtual Ordene Orden { get; set; } = null!;
}
