using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Postulacion
{
    public int IdPostulacion { get; set; }

    public int IdOferta { get; set; }

    public int IdAlumno { get; set; }

    public int IdEstadoPostulacion { get; set; }

    public DateTime FechaPostulacion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual EstadoPostulacion IdEstadoPostulacionNavigation { get; set; } = null!;

    public virtual OfertaLaboral IdOfertaNavigation { get; set; } = null!;
}
