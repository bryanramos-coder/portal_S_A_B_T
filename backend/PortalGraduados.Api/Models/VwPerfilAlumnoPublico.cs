using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwPerfilAlumnoPublico
{
    public int? IdAlumno { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? FotoUrl { get; set; }

    public string? SituacionAcademica { get; set; }

    public short? AnioEgresoGraduacion { get; set; }

    public string? AreaEspecializacion { get; set; }

    public string? CondicionLaboral { get; set; }

    public decimal? AniosExperiencia { get; set; }

    public List<string>? Habilidades { get; set; }
}
