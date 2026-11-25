using System.Text.Json.Serialization;

namespace PortalGraduados.Api.Dtos;

public class LoginRequest
{
    public string Correo { get; set; } = "";
    public string Clave { get; set; } = "";
}

public class LoginResponse
{
    public string Estado { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Rol { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Token { get; set; }
}

public class RegistroAlumnoRequest
{
    public string Apellido { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Carnet { get; set; } = "";
    public string Situacion { get; set; } = "";
    public string Anio { get; set; } = "";
    public string Clave { get; set; } = "";
}

public class RegistroEmpresaRequest
{
    public string Empresa { get; set; } = "";
    public string Correo { get; set; } = "";
    public string ContactoNombres { get; set; } = "";
    public string ContactoApellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Web { get; set; } = "";
    public string Clave { get; set; } = "";
    public bool Terminos { get; set; }
}
