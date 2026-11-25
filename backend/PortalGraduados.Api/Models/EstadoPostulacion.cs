using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class EstadoPostulacion
{
    public int IdEstadoPostulacion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Postulacion> Postulacions { get; set; } = new List<Postulacion>();
}
