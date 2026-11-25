using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Aviso
{
    public int IdAviso { get; set; }

    public int IdOferta { get; set; }

    public int IdAlumno { get; set; }

    public DateTime FechaEnvio { get; set; }

    public string EstadoEnvio { get; set; } = null!;

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual OfertaLaboral IdOfertaNavigation { get; set; } = null!;
}
