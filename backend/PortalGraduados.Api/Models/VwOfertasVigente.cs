using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwOfertasVigente
{
    public int? IdOferta { get; set; }

    public int? IdEmpresa { get; set; }

    public string? Titulo { get; set; }

    public string? Empresa { get; set; }

    public string? Sector { get; set; }

    public string? Departamento { get; set; }

    public string? DetalleUbicacion { get; set; }

    public string? TipoEmpleo { get; set; }

    public string? Modalidad { get; set; }

    public decimal? SalarioMin { get; set; }

    public decimal? SalarioMax { get; set; }

    public DateOnly? FechaPublicacion { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public short? VacantesDisponibles { get; set; }

    public int? DiasRestantes { get; set; }

    public List<string>? Habilidades { get; set; }
}
