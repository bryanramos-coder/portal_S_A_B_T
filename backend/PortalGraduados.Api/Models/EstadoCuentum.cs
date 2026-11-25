using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class EstadoCuentum
{
    public int IdEstadoCuenta { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
