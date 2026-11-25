using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Correo { get; set; } = null!;

    public string ContraseniaHash { get; set; } = null!;

    public int IdRol { get; set; }

    public int IdEstadoCuenta { get; set; }

    public virtual Alumno? Alumno { get; set; }

    public virtual DatosRegistro? DatosRegistro { get; set; }

    public virtual Empresa? Empresa { get; set; }

    public virtual EstadoCuentum IdEstadoCuentaNavigation { get; set; } = null!;

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<OfertaLaboral> OfertaLaborals { get; set; } = new List<OfertaLaboral>();

    public virtual ICollection<SolicitudCuentum> SolicitudCuentumIdRevisorNavigations { get; set; } = new List<SolicitudCuentum>();

    public virtual SolicitudCuentum? SolicitudCuentumIdUsuarioNavigation { get; set; }
}
