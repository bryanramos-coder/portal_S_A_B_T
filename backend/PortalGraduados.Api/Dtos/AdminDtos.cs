using System.Text.Json.Serialization;

namespace PortalGraduados.Api.Dtos;

public class UsuarioAdminDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string FechaRegistro { get; set; } = "";
    public string Telefono { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Carnet { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Situacion { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Anio { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContactoNombres { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContactoApellidos { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CargoContacto { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SitioWeb { get; set; }
}

public class CambiarEstadoUsuarioRequest
{
    public string Accion { get; set; } = "";
}

public class SeguimientoHistorialDto
{
    public string Cargo { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Desde { get; set; } = "";
    public string? Hasta { get; set; }
}

public class SeguimientoPerfilDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Condicion { get; set; } = "";
    public int Anio { get; set; }
    public string AreaEspecializacion { get; set; } = "";
    public bool Empleado { get; set; }
    public string PuestoActual { get; set; } = "";
    public string EmpresaActual { get; set; } = "";
    public string Sector { get; set; } = "";
    public string InicioPuestoActual { get; set; } = "";
    public List<string> Tecnologias { get; set; } = new();
    public bool PerfilCompleto { get; set; }
    public List<SeguimientoHistorialDto> Historial { get; set; } = new();
}

public class ResumenPostulacionesDto
{
    public int Total { get; set; }
    public int EnCurso { get; set; }
    public int Seleccionados { get; set; }
}

public class ResumenAdminDto
{
    public int EgresadosRegistrados { get; set; }
    public int EmpresasActivas { get; set; }
    public int OfertasPublicadas { get; set; }
    public int AlumnosRegistrados { get; set; }
    public ResumenPostulacionesDto Postulaciones { get; set; } = new();
}
