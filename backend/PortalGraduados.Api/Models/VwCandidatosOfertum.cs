using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwCandidatosOfertum
{
    public int? IdPostulacion { get; set; }

    public int? IdOferta { get; set; }

    public int? IdEmpresa { get; set; }

    public int? IdPublicador { get; set; }

    public int? IdAlumno { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? SituacionAcademica { get; set; }

    public decimal? AniosExperiencia { get; set; }

    public decimal? Afinidad { get; set; }

    public string? EstadoPostulacion { get; set; }

    public DateTime? FechaPostulacion { get; set; }

    public DateTime? FechaResolucion { get; set; }
}
