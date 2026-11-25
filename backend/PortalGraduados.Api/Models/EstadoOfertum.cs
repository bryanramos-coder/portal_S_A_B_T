using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class EstadoOfertum
{
    public int IdEstadoOferta { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<OfertaLaboral> OfertaLaborals { get; set; } = new List<OfertaLaboral>();
}
