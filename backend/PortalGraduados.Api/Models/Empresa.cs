using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Empresa
{
    public int IdEmpresa { get; set; }

    public int IdUsuario { get; set; }

    public string NombreEmpresa { get; set; } = null!;

    public string? Nit { get; set; }

    public string? SitioWeb { get; set; }

    public string? Descripcion { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<OfertaLaboral> OfertaLaborals { get; set; } = new List<OfertaLaboral>();
}
