using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwEmpleabilidadPorAnio
{
    public short? AnioEgresoGraduacion { get; set; }

    public string? SituacionAcademica { get; set; }

    public long? TotalAlumnos { get; set; }

    public long? AlumnosConEmpleo { get; set; }

    public decimal? PorcentajeEmpleabilidad { get; set; }
}
