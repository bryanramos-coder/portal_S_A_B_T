using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwMisPostulacione
{
    public int? IdPostulacion { get; set; }

    public int? IdAlumno { get; set; }

    public int? IdOferta { get; set; }

    public string? Titulo { get; set; }

    public string? Empresa { get; set; }

    public string? EstadoPostulacion { get; set; }

    public string? EstadoOferta { get; set; }

    public DateTime? FechaPostulacion { get; set; }

    public DateTime? FechaResolucion { get; set; }
}
