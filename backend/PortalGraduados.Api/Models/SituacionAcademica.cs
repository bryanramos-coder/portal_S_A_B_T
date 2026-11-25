using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class SituacionAcademica
{
    public int IdSituacion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
}
