using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class OfertaLaboral
{
    public int IdOferta { get; set; }

    public int? IdEmpresa { get; set; }

    public string? EmpresaExterna { get; set; }

    public int IdPublicador { get; set; }

    public int IdSector { get; set; }

    public int IdEstadoOferta { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mision { get; set; } = null!;

    public string Funciones { get; set; } = null!;

    public string Requisitos { get; set; } = null!;

    public string? Competencias { get; set; }

    public string? Horarios { get; set; }

    public string? Ofrecemos { get; set; }

    public short PuestosVacantes { get; set; }

    public short PuestosCubiertos { get; set; }

    public string NivelExperiencia { get; set; } = null!;

    public string Departamento { get; set; } = null!;

    public string? DetalleUbicacion { get; set; }

    public string TipoEmpleo { get; set; } = null!;

    public string? Modalidad { get; set; }

    public string? Vehiculo { get; set; }

    public decimal? SalarioMin { get; set; }

    public decimal? SalarioMax { get; set; }

    public string? PersonaContacto { get; set; }

    public string? CorreoContacto { get; set; }

    public string? Indicaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateOnly? FechaPublicacion { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual EstadoOfertum IdEstadoOfertaNavigation { get; set; } = null!;

    public virtual Usuario IdPublicadorNavigation { get; set; } = null!;

    public virtual Sector IdSectorNavigation { get; set; } = null!;

    public virtual ICollection<Postulacion> Postulacions { get; set; } = new List<Postulacion>();

    public virtual ICollection<Habilidad> IdHabilidads { get; set; } = new List<Habilidad>();
}
