using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class EstadoSolicitud
{
    public int IdEstadoSolicitud { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<SolicitudCuentum> SolicitudCuenta { get; set; } = new List<SolicitudCuentum>();
}
