using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Habilidad
{
    public int IdHabilidad { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Alumno> IdAlumnos { get; set; } = new List<Alumno>();

    public virtual ICollection<OfertaLaboral> IdOferta { get; set; } = new List<OfertaLaboral>();
}
