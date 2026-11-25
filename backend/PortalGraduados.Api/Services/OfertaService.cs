using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Dtos;
using PortalGraduados.Api.Helpers;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Services;

public class OfertaService
{
    private const decimal SalarioLimite = 99999999.99m;

    public async Task<Dictionary<string, string>> ValidarOfertaAsync(
        PortalDbContext db,
        FormOfertaRequest form,
        bool esExterna,
        bool paraPublicar)
    {
        var errores = new Dictionary<string, string>();
        DateOnly hoy = Validaciones.HoyLocal();

        if (string.IsNullOrWhiteSpace(form.Puesto) || form.Puesto.Trim().Length > 150)
            errores["puesto"] = "Escribe el puesto.";

        if (esExterna && (string.IsNullOrWhiteSpace(form.Empresa) || form.Empresa.Trim().Length > 150))
            errores["empresa"] = "Escribe el nombre de la empresa.";

        if (string.IsNullOrWhiteSpace(form.Sector))
        {
            errores["sector"] = "Selecciona el sector.";
        }
        else
        {
            bool existeSector = await db.Sectors.AnyAsync(s => s.Nombre == form.Sector.Trim());
            if (!existeSector)
                errores["sector"] = "Selecciona un sector válido.";
        }

        if (string.IsNullOrWhiteSpace(form.Departamento) || !Constantes.Departamentos.Contains(form.Departamento.Trim()))
            errores["departamento"] = "Selecciona el departamento.";

        if (string.IsNullOrWhiteSpace(form.Tipo) || !Constantes.TiposEmpleo.Contains(form.Tipo.Trim()))
            errores["tipo"] = "Selecciona el tipo de contratación.";

        if (string.IsNullOrWhiteSpace(form.NivelExperiencia) || !Constantes.NivelesExperiencia.Contains(form.NivelExperiencia.Trim()))
            errores["nivelExperiencia"] = "Selecciona el nivel de experiencia.";

        if (!string.IsNullOrWhiteSpace(form.Vehiculo) && !Constantes.Vehiculos.Contains(form.Vehiculo.Trim()))
            errores["vehiculo"] = "Selecciona una opción de vehículo válida.";

        if (!string.IsNullOrWhiteSpace(form.Modalidad) && !Constantes.Modalidades.Contains(form.Modalidad.Trim()))
            errores["modalidad"] = "Selecciona una modalidad válida.";

        if (form.Vacantes < 1 || form.Vacantes > 32767)
            errores["vacantes"] = "Indica un número entero de 1 a 32,767.";

        if (string.IsNullOrWhiteSpace(form.FechaExpiracion))
        {
            errores["fechaExpiracion"] = "Selecciona la fecha de expiración.";
        }
        else if (!DateOnly.TryParseExact(form.FechaExpiracion.Trim(), "yyyy-MM-dd", out DateOnly fechaExp))
        {
            errores["fechaExpiracion"] = "Formato de fecha inválido (AAAA-MM-DD).";
        }
        else if (fechaExp <= hoy)
        {
            errores["fechaExpiracion"] = "La fecha debe ser posterior a hoy.";
        }

        if (string.IsNullOrWhiteSpace(form.Objetivo))
            errores["objetivo"] = "Describe el objetivo del puesto.";

        if (string.IsNullOrWhiteSpace(form.Funciones))
            errores["funciones"] = "Escribe al menos una función.";

        if (string.IsNullOrWhiteSpace(form.Requisitos))
            errores["requisitos"] = "Escribe al menos un requisito.";

        if (form.SalarioMin.HasValue && (form.SalarioMin < 0 || form.SalarioMin > SalarioLimite))
            errores["salarioMin"] = "Escribe un monto entre 0 y 99,999,999.99.";

        if (form.SalarioMax.HasValue && (form.SalarioMax < 0 || form.SalarioMax > SalarioLimite))
            errores["salarioMax"] = "Escribe un monto entre 0 y 99,999,999.99.";

        if (form.SalarioMin.HasValue && form.SalarioMax.HasValue &&
            form.SalarioMax < form.SalarioMin &&
            !errores.ContainsKey("salarioMin") && !errores.ContainsKey("salarioMax"))
        {
            errores["salarioMax"] = "El salario máximo no puede ser menor que el mínimo.";
        }

        if (!string.IsNullOrWhiteSpace(form.ContactoCorreo) && !Validaciones.EsCorreoValido(form.ContactoCorreo))
            errores["contactoCorreo"] = "Escribe un correo válido.";

        List<string> habilidadesLimpias = NormalizarHabilidades(form.Habilidades);
        if (paraPublicar && habilidadesLimpias.Count == 0)
            errores["habilidades"] = "Agrega al menos una habilidad para poder publicar.";

        return errores;
    }

    public static List<string> NormalizarHabilidades(IEnumerable<string>? lista)
    {
        var resultado = new List<string>();
        if (lista == null) return resultado;

        foreach (string item in lista)
        {
            string limpio = Regex.Replace((item ?? "").Trim(), @"\s+", " ");
            if (limpio.Length == 0 || limpio.Length > 100) continue;
            if (!resultado.Any(x => x.Equals(limpio, StringComparison.OrdinalIgnoreCase)))
                resultado.Add(limpio);
        }

        return resultado;
    }

    public async Task<List<Habilidad>> ObtenerOCrearHabilidadesAsync(PortalDbContext db, IEnumerable<string>? nombres)
    {
        List<string> limpias = NormalizarHabilidades(nombres);
        var resultado = new List<Habilidad>();
        if (limpias.Count == 0) return resultado;

        List<Habilidad> existentes = await db.Habilidads.ToListAsync();

        foreach (string nombre in limpias)
        {
            Habilidad? encontrada = existentes.FirstOrDefault(
                h => h.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (encontrada == null)
            {
                encontrada = new Habilidad { Nombre = nombre };
                db.Habilidads.Add(encontrada);
                existentes.Add(encontrada);
            }

            resultado.Add(encontrada);
        }

        return resultado;
    }

    public static OfertaDto MapearOferta(OfertaLaboral o, bool incluirConteoPostulantes = false)
    {
        string nombreEmpresa = o.IdEmpresaNavigation != null
            ? o.IdEmpresaNavigation.NombreEmpresa
            : (o.EmpresaExterna ?? "");

        string logo = o.IdEmpresaNavigation?.IdUsuarioNavigation?.DatosRegistro?.FotoUrl ?? "";
        string origen = o.IdEmpresa != null ? Constantes.RolEmpresa : Constantes.RolAdmin;

        int? postulantes = null;
        if (incluirConteoPostulantes)
        {
            postulantes = o.Postulacions.Count(p => p.IdEstadoPostulacionNavigation.Nombre != Constantes.PostulacionRetirada);
        }

        return new OfertaDto
        {
            Id = o.IdOferta,
            Puesto = o.Titulo,
            Empresa = nombreEmpresa,
            Sector = o.IdSectorNavigation.Nombre,
            Departamento = o.Departamento,
            UbicacionDetalle = o.DetalleUbicacion ?? "",
            Tipo = o.TipoEmpleo,
            NivelExperiencia = o.NivelExperiencia,
            Vacantes = o.PuestosVacantes,
            Cubiertas = o.PuestosCubiertos,
            Vehiculo = o.Vehiculo ?? "",
            Modalidad = o.Modalidad ?? "",
            SalarioMin = o.SalarioMin,
            SalarioMax = o.SalarioMax,
            Habilidades = o.IdHabilidads.Select(h => h.Nombre).OrderBy(n => n).ToList(),
            Objetivo = o.Mision,
            Funciones = o.Funciones,
            Requisitos = o.Requisitos,
            Competencias = o.Competencias ?? "",
            Horarios = o.Horarios ?? "",
            Ofrecemos = o.Ofrecemos ?? "",
            ContactoNombre = o.PersonaContacto ?? "",
            ContactoCorreo = o.CorreoContacto ?? "",
            Indicaciones = o.Indicaciones ?? "",
            FechaPublicacion = o.FechaPublicacion?.ToString("yyyy-MM-dd") ?? "",
            FechaExpiracion = o.FechaVencimiento.ToString("yyyy-MM-dd"),
            Estado = o.IdEstadoOfertaNavigation.Nombre,
            Origen = origen,
            Logo = logo,
            Postulantes = postulantes
        };
    }

    public static PostulacionDto MapearPostulacion(Postulacion p)
    {
        OfertaLaboral oferta = p.IdOfertaNavigation;
        Alumno alumno = p.IdAlumnoNavigation;
        DatosRegistro reg = alumno.IdUsuarioNavigation.DatosRegistro!;

        string nombreEmpresa = oferta.IdEmpresaNavigation != null
            ? oferta.IdEmpresaNavigation.NombreEmpresa
            : (oferta.EmpresaExterna ?? "");

        return new PostulacionDto
        {
            Id = p.IdPostulacion,
            IdOferta = p.IdOferta,
            Puesto = oferta.Titulo,
            Empresa = nombreEmpresa,
            Vacantes = oferta.PuestosVacantes,
            Nombres = reg.Nombres,
            Apellidos = reg.Apellidos,
            Correo = alumno.IdUsuarioNavigation.Correo,
            Telefono = reg.Telefono,
            Situacion = alumno.IdSituacionNavigation.Nombre,
            Anio = alumno.AnioEgresoGraduacion?.ToString() ?? "",
            Area = alumno.IdAreaEspNavigation?.Nombre ?? "",
            Habilidades = alumno.IdHabilidads.Select(h => h.Nombre).OrderBy(n => n).ToList(),
            Experiencias = alumno.ExperienciaLaborals
                .OrderByDescending(e => e.FechaInicio)
                .Select(e => new ExperienciaDto
                {
                    Cargo = e.Cargo,
                    Empresa = e.Empresa,
                    Sector = e.IdSectorNavigation?.Nombre ?? "",
                    Responsabilidades = e.Responsabilidades,
                    Desde = e.FechaInicio.ToString("yyyy-MM"),
                    Hasta = e.FechaFin?.ToString("yyyy-MM") ?? ""
                })
                .ToList(),
            Estado = p.IdEstadoPostulacionNavigation.Nombre,
            FechaPostulacion = Validaciones.FormatearFechaHora(p.FechaPostulacion),
            FechaResolucion = Validaciones.FormatearFechaHora(p.FechaResolucion)
        };
    }
}
