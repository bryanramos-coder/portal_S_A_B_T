namespace PortalGraduados.Api.Dtos;

public class PostulacionDto
{
    public int Id { get; set; }
    public int IdOferta { get; set; }
    public string Puesto { get; set; } = "";
    public string Empresa { get; set; } = "";
    public int Vacantes { get; set; }
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Situacion { get; set; } = "";
    public string Anio { get; set; } = "";
    public string Area { get; set; } = "";
    public List<string> Habilidades { get; set; } = new();
    public List<ExperienciaDto> Experiencias { get; set; } = new();
    public string Estado { get; set; } = "";
    public string FechaPostulacion { get; set; } = "";
    public string FechaResolucion { get; set; } = "";
}

public class AplicarRequest
{
    public int IdOferta { get; set; }
}

public class CambiarEstadoPostulacionRequest
{
    public string Estado { get; set; } = "";
}
