using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwSolicitudesPendiente
{
    public int? IdSolicitud { get; set; }

    public int? IdUsuario { get; set; }

    public string? Correo { get; set; }

    public string? Rol { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? Telefono { get; set; }

    public string? Cargo { get; set; }

    public string? Carnet { get; set; }

    public string? SituacionAcademica { get; set; }

    public string? NombreEmpresa { get; set; }

    public string? Nit { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public int? DiasEspera { get; set; }

    public bool? DeclaracionVeracidad { get; set; }
}
