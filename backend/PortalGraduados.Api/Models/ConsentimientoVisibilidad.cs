using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class ConsentimientoVisibilidad
{
    public int IdConsentimiento { get; set; }

    public int IdAlumno { get; set; }

    public DateTime FechaOtorgado { get; set; }

    public DateTime? FechaRevocado { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
}
