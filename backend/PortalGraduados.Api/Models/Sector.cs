using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Sector
{
    public int IdSector { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<ExperienciaLaboral> ExperienciaLaborals { get; set; } = new List<ExperienciaLaboral>();

    public virtual ICollection<OfertaLaboral> OfertaLaborals { get; set; } = new List<OfertaLaboral>();
}
