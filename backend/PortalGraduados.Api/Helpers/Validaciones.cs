using System.Text.RegularExpressions;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Helpers;

public static class Validaciones
{
    private static readonly Regex RegexCorreo = new(@"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefono = new(@"^\+?(?=(?:[^0-9]*[0-9]){8,15}$)[0-9][0-9\s\-]*[0-9]$", RegexOptions.Compiled);
    private static readonly Regex RegexCarnet = new(@"^[A-Z]{2}[0-9]{3,6}$", RegexOptions.Compiled);
    private static readonly Regex RegexClaveEmpresa = new(@"^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$", RegexOptions.Compiled);
    private static readonly Regex RegexAnio = new(@"^\d{4}$", RegexOptions.Compiled);
    private static readonly Regex RegexMes = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

    public static bool EsCorreoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo)) return false;
        string limpio = correo.Trim();
        return limpio.Length <= 150 && RegexCorreo.IsMatch(limpio);
    }

    public static bool EsTelefonoValido(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return false;
        string limpio = telefono.Trim();
        return limpio.Length <= 20 && RegexTelefono.IsMatch(limpio);
    }

    public static bool EsCarnetValido(string? carnet)
    {
        if (string.IsNullOrWhiteSpace(carnet)) return false;
        return RegexCarnet.IsMatch(carnet.Trim().ToUpperInvariant());
    }

    public static bool EsClaveEmpresaValida(string? clave)
    {
        if (string.IsNullOrEmpty(clave)) return false;
        return RegexClaveEmpresa.IsMatch(clave);
    }

    public static bool EsNitValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit)) return false;
        string limpio = nit.Trim();
        if (limpio.Length > 20) return false;
        int digitos = limpio.Count(char.IsDigit);
        return digitos == 14;
    }

    public static bool EsAnioValido(string? anioTexto, out short anio)
    {
        anio = 0;
        if (string.IsNullOrWhiteSpace(anioTexto)) return false;
        string limpio = anioTexto.Trim();
        if (!RegexAnio.IsMatch(limpio)) return false;
        if (!short.TryParse(limpio, out anio)) return false;
        return anio >= 1960 && anio <= 2100;
    }

    public static bool TryParseMes(string? mesTexto, out DateOnly fecha)
    {
        fecha = default;
        if (string.IsNullOrWhiteSpace(mesTexto)) return false;
        string limpio = mesTexto.Trim();
        if (!RegexMes.IsMatch(limpio)) return false;
        return DateOnly.TryParseExact(limpio + "-01", "yyyy-MM-dd", out fecha);
    }

    public static string? LimpiarNulo(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        return valor.Trim();
    }

    public static DateOnly HoyLocal()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));
    }

    public static DateTime AhoraSinZona()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }

    public static string FormatearFechaHora(DateTime? fecha)
    {
        if (fecha == null) return "";
        return DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    public static bool PerfilEmpresaCompleto(Empresa empresa, DatosRegistro registro)
    {
        return !string.IsNullOrWhiteSpace(empresa.NombreEmpresa)
            && !string.IsNullOrWhiteSpace(empresa.Nit)
            && !string.IsNullOrWhiteSpace(empresa.Descripcion)
            && !string.IsNullOrWhiteSpace(registro.Nombres)
            && !string.IsNullOrWhiteSpace(registro.Apellidos)
            && !string.IsNullOrWhiteSpace(registro.Telefono);
    }

    public static bool PerfilAlumnoCompleto(Alumno alumno, DatosRegistro registro, string situacionNombre)
    {
        if (string.IsNullOrWhiteSpace(registro.Nombres)
            || string.IsNullOrWhiteSpace(registro.Apellidos)
            || string.IsNullOrWhiteSpace(registro.Telefono)
            || string.IsNullOrWhiteSpace(alumno.Carnet))
        {
            return false;
        }

        if (situacionNombre != "Quinto año" && alumno.AnioEgresoGraduacion == null)
        {
            return false;
        }

        if (situacionNombre == "Graduado" && alumno.IdAreaEsp == null)
        {
            return false;
        }

        return true;
    }
}
