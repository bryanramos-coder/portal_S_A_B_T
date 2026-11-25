using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class VwResumenOferta
{
    public int? IdOferta { get; set; }

    public int? IdEmpresa { get; set; }

    public int? IdPublicador { get; set; }

    public string? Titulo { get; set; }

    public string? Empresa { get; set; }

    public string? EstadoOferta { get; set; }

    public short? PuestosVacantes { get; set; }

    public short? PuestosCubiertos { get; set; }

    public short? VacantesDisponibles { get; set; }

    public long? Recibidas { get; set; }

    public long? EnRevision { get; set; }

    public long? Seleccionadas { get; set; }

    public long? Descartadas { get; set; }

    public long? Retiradas { get; set; }
}
