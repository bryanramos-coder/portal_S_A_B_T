using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class ExperienciaLaboral
{
    public int IdExperiencia { get; set; }

    public int IdAlumno { get; set; }

    public int? IdSector { get; set; }

    public string Empresa { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public string Responsabilidades { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool EsActual { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual Sector? IdSectorNavigation { get; set; }
}
