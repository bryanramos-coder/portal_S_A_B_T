using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class SolicitudCuentum
{
    public int IdSolicitud { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoSolicitud { get; set; }

    public int? IdRevisor { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public bool? DeclaracionVeracidad { get; set; }

    public virtual EstadoSolicitud IdEstadoSolicitudNavigation { get; set; } = null!;

    public virtual Usuario? IdRevisorNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
