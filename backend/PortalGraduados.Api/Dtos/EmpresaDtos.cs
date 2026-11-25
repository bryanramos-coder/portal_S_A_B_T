namespace PortalGraduados.Api.Dtos;

public class PerfilEmpresaDto
{
    public string NombreEmpresa { get; set; } = "";
    public string Nit { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string SitioWeb { get; set; } = "";
    public string ContactoNombres { get; set; } = "";
    public string ContactoApellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string CargoContacto { get; set; } = "";
    public string Correo { get; set; } = "";
    public string LogoUrl { get; set; } = "";
}

public class ActualizarPerfilEmpresaRequest
{
    public string NombreEmpresa { get; set; } = "";
    public string Nit { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string SitioWeb { get; set; } = "";
    public string ContactoNombres { get; set; } = "";
    public string ContactoApellidos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string CargoContacto { get; set; } = "";
}

public class LogoResponse
{
    public string LogoUrl { get; set; } = "";
}
