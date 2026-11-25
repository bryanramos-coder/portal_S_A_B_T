using System.Text.Json.Serialization;

namespace PortalGraduados.Api.Dtos;

public class OfertaDto
{
    public int Id { get; set; }
    public string Puesto { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Departamento { get; set; } = "";
    public string UbicacionDetalle { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string NivelExperiencia { get; set; } = "";
    public int Vacantes { get; set; }
    public int Cubiertas { get; set; }
    public string Vehiculo { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public decimal? SalarioMin { get; set; }
    public decimal? SalarioMax { get; set; }
    public List<string> Habilidades { get; set; } = new();
    public string Objetivo { get; set; } = "";
    public string Funciones { get; set; } = "";
    public string Requisitos { get; set; } = "";
    public string Competencias { get; set; } = "";
    public string Horarios { get; set; } = "";
    public string Ofrecemos { get; set; } = "";
    public string ContactoNombre { get; set; } = "";
    public string ContactoCorreo { get; set; } = "";
    public string Indicaciones { get; set; } = "";
    public string FechaPublicacion { get; set; } = "";
    public string FechaExpiracion { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Origen { get; set; } = "";
    public string Logo { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Postulantes { get; set; }
}

public class FormOfertaRequest
{
    public string Puesto { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Departamento { get; set; } = "";
    public string UbicacionDetalle { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string NivelExperiencia { get; set; } = "";
    public int Vacantes { get; set; }
    public string Vehiculo { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public decimal? SalarioMin { get; set; }
    public decimal? SalarioMax { get; set; }
    public List<string> Habilidades { get; set; } = new();
    public string Objetivo { get; set; } = "";
    public string Funciones { get; set; } = "";
    public string Requisitos { get; set; } = "";
    public string Competencias { get; set; } = "";
    public string Horarios { get; set; } = "";
    public string Ofrecemos { get; set; } = "";
    public string ContactoNombre { get; set; } = "";
    public string ContactoCorreo { get; set; } = "";
    public string Indicaciones { get; set; } = "";
    public string FechaExpiracion { get; set; } = "";
}
