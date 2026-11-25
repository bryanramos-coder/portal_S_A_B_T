namespace PortalGraduados.Api.Services;

public class ArchivoService
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly Dictionary<string, string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        { "image/jpeg", ".jpg" },
        { "image/png", ".png" },
        { "image/webp", ".webp" }
    };
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _env;

    public ArchivoService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(bool Ok, string? Url, string? Error)> GuardarImagenAsync(IFormFile? archivo, string subcarpeta, int idUsuario)
    {
        if (archivo == null || archivo.Length == 0)
            return (false, null, "Debes seleccionar una imagen.");

        if (archivo.Length > MaxBytes)
            return (false, null, "La imagen no debe pasar de 5 MB.");

        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!TiposPermitidos.TryGetValue(archivo.ContentType, out string? extPorMime) ||
            !ExtensionesPermitidas.Contains(extension))
        {
            return (false, null, "La imagen debe ser PNG, JPG o WEBP.");
        }

        string extFinal = extension == ".jpeg" ? ".jpg" : extPorMime;
        string webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        string carpeta = Path.Combine(webRoot, "uploads", subcarpeta);
        Directory.CreateDirectory(carpeta);

        EliminarArchivosUsuario(carpeta, idUsuario);

        string nombreArchivo = $"{idUsuario}{extFinal}";
        string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        await using var stream = new FileStream(rutaCompleta, FileMode.Create);
        await archivo.CopyToAsync(stream);

        return (true, $"/uploads/{subcarpeta}/{nombreArchivo}", null);
    }

    public void EliminarImagen(string subcarpeta, int idUsuario)
    {
        string webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        string carpeta = Path.Combine(webRoot, "uploads", subcarpeta);
        if (Directory.Exists(carpeta))
        {
            EliminarArchivosUsuario(carpeta, idUsuario);
        }
    }

    private static void EliminarArchivosUsuario(string carpeta, int idUsuario)
    {
        foreach (string archivo in Directory.GetFiles(carpeta, $"{idUsuario}.*"))
        {
            File.Delete(archivo);
        }
    }
}
