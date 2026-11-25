using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Alumno
{
    public int IdAlumno { get; set; }

    public int IdUsuario { get; set; }

    public int IdSituacion { get; set; }

    public int? IdAreaEsp { get; set; }

    public string Carnet { get; set; } = null!;

    public short? AnioEgresoGraduacion { get; set; }

    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();

    public virtual ICollection<ConsentimientoVisibilidad> ConsentimientoVisibilidads { get; set; } = new List<ConsentimientoVisibilidad>();

    public virtual ICollection<ExperienciaLaboral> ExperienciaLaborals { get; set; } = new List<ExperienciaLaboral>();

    public virtual AreaEspecializacion? IdAreaEspNavigation { get; set; }

    public virtual SituacionAcademica IdSituacionNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Postulacion> Postulacions { get; set; } = new List<Postulacion>();

    public virtual ICollection<Habilidad> IdHabilidads { get; set; } = new List<Habilidad>();
}
