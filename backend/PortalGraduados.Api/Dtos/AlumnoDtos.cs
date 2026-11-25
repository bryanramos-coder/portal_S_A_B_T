namespace PortalGraduados.Api.Dtos;

public class ExperienciaDto
{
    public string Cargo { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Responsabilidades { get; set; } = "";
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
}

public class PerfilAlumnoDto
{
    public string Situacion { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Carnet { get; set; } = "";
    public string AreaEsp { get; set; } = "";
    public string Anio { get; set; } = "";
    public List<string> Habilidades { get; set; } = new();
    public List<ExperienciaDto> Experiencias { get; set; } = new();
    public bool Visible { get; set; }
    public string VisibleDesde { get; set; } = "";
    public string Correo { get; set; } = "";
    public string FotoUrl { get; set; } = "";
}

public class ActualizarPerfilAlumnoRequest
{
    public string Situacion { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Carnet { get; set; } = "";
    public string AreaEsp { get; set; } = "";
    public string Anio { get; set; } = "";
    public List<string> Habilidades { get; set; } = new();
    public List<ExperienciaDto> Experiencias { get; set; } = new();
    public bool Visible { get; set; }
}

public class FotoResponse
{
    public string FotoUrl { get; set; } = "";
}
