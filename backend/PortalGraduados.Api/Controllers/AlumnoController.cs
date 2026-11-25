using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Dtos;
using PortalGraduados.Api.Helpers;
using PortalGraduados.Api.Models;
using PortalGraduados.Api.Services;

namespace PortalGraduados.Api.Controllers;

[ApiController]
[Route("api/alumno")]
[Authorize(Roles = Constantes.RolAlumno)]
public class AlumnoController : ControllerBase
{
    private readonly PortalDbContext _db;
    private readonly OfertaService _ofertaService;
    private readonly ArchivoService _archivoService;

    public AlumnoController(PortalDbContext db, OfertaService ofertaService, ArchivoService archivoService)
    {
        _db = db;
        _ofertaService = ofertaService;
        _archivoService = archivoService;
    }

    private int ObtenerIdUsuario()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("perfil")]
    public async Task<ActionResult<PerfilAlumnoDto>> ObtenerPerfil()
    {
        int userId = ObtenerIdUsuario();

        Alumno? alumno = await QueryAlumnoPerfil(asNoTracking: true)
            .FirstOrDefaultAsync(a => a.IdUsuario == userId);

        if (alumno == null)
            return NotFound(new { mensaje = "Perfil de alumno no encontrado." });

        return Ok(MapearPerfilAlumno(alumno));
    }

    [HttpPut("perfil")]
    public async Task<ActionResult<PerfilAlumnoDto>> ActualizarPerfil([FromBody] ActualizarPerfilAlumnoRequest req)
    {
        int userId = ObtenerIdUsuario();

        Alumno? alumno = await QueryAlumnoPerfil(asNoTracking: false)
            .FirstOrDefaultAsync(a => a.IdUsuario == userId);

        if (alumno == null)
            return NotFound(new { mensaje = "Perfil de alumno no encontrado." });

        var campos = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(req.Nombres) || req.Nombres.Trim().Length > 100)
            campos["nombres"] = "Escribe tus nombres.";

        if (string.IsNullOrWhiteSpace(req.Apellidos) || req.Apellidos.Trim().Length > 100)
            campos["apellidos"] = "Escribe tus apellidos.";

        if (!Validaciones.EsTelefonoValido(req.Telefono))
            campos["telefono"] = "Escribe un teléfono válido (de 8 a 15 dígitos).";

        string carnetLimpio = (req.Carnet ?? "").Trim().ToUpperInvariant();
        if (!Validaciones.EsCarnetValido(carnetLimpio))
            campos["carnet"] = "El carnet debe iniciar con dos letras seguidas de 3 a 6 números (Ej: RC23011).";

        SituacionAcademica? situacion = await _db.SituacionAcademicas
            .FirstOrDefaultAsync(s => s.Nombre == (req.Situacion ?? "").Trim());

        if (situacion == null)
            campos["situacion"] = "Selecciona una situación académica válida.";
        else if (situacion.IdSituacion < alumno.IdSituacion)
            campos["situacion"] = "La situación académica no puede retroceder.";

        short? anioGuardado = null;
        if (situacion != null && situacion.IdSituacion != 1)
        {
            if (string.IsNullOrWhiteSpace(req.Anio))
            {
                campos["anio"] = "Indica el año de egreso o graduación.";
            }
            else if (!Validaciones.EsAnioValido(req.Anio, out short anioParsed))
            {
                campos["anio"] = "Escribe un año entre 1960 y 2100.";
            }
            else
            {
                anioGuardado = anioParsed;
            }
        }

        int? idAreaEsp = null;
        if (!string.IsNullOrWhiteSpace(req.AreaEsp))
        {
            AreaEspecializacion? area = await _db.AreaEspecializacions
                .FirstOrDefaultAsync(a => a.Nombre == req.AreaEsp.Trim());
            if (area == null)
                campos["areaEsp"] = "Selecciona un área de especialización válida.";
            else
                idAreaEsp = area.IdAreaEsp;
        }

        DateOnly hoy = Validaciones.HoyLocal();
        var experienciasPreparadas = new List<(string Cargo, string Empresa, int? IdSector, string Resp, DateOnly Desde, DateOnly? Hasta, bool EsActual)>();
        List<Sector> sectoresDb = await _db.Sectors.ToListAsync();

        for (int i = 0; i < req.Experiencias.Count; i++)
        {
            ExperienciaDto exp = req.Experiencias[i];
            if (string.IsNullOrWhiteSpace(exp.Cargo) || exp.Cargo.Trim().Length > 100 ||
                string.IsNullOrWhiteSpace(exp.Empresa) || exp.Empresa.Trim().Length > 150 ||
                string.IsNullOrWhiteSpace(exp.Responsabilidades) ||
                string.IsNullOrWhiteSpace(exp.Desde))
            {
                campos[$"exp-{i}"] = "Completa el cargo, la empresa, las responsabilidades y la fecha de inicio.";
                continue;
            }

            if (!Validaciones.TryParseMes(exp.Desde, out DateOnly fechaDesde))
            {
                campos[$"exp-{i}"] = "Formato de fecha de inicio inválido (AAAA-MM).";
                continue;
            }

            if (fechaDesde > hoy)
            {
                campos[$"exp-{i}"] = "La fecha de inicio no puede ser futura.";
                continue;
            }

            DateOnly? fechaHasta = null;
            bool esActual = string.IsNullOrWhiteSpace(exp.Hasta);

            if (!esActual)
            {
                if (!Validaciones.TryParseMes(exp.Hasta, out DateOnly fechaFinParsed))
                {
                    campos[$"exp-{i}"] = "Formato de fecha final inválido (AAAA-MM).";
                    continue;
                }
                if (fechaFinParsed < fechaDesde)
                {
                    campos[$"exp-{i}"] = "La fecha final no puede ser anterior a la de inicio.";
                    continue;
                }
                fechaHasta = fechaFinParsed;
            }

            int? idSectorExp = null;
            if (!string.IsNullOrWhiteSpace(exp.Sector))
            {
                Sector? sec = sectoresDb.FirstOrDefault(s => s.Nombre == exp.Sector.Trim());
                if (sec == null)
                {
                    campos[$"exp-{i}"] = "Sector laboral no válido.";
                    continue;
                }
                idSectorExp = sec.IdSector;
            }

            experienciasPreparadas.Add((
                exp.Cargo.Trim(),
                exp.Empresa.Trim(),
                idSectorExp,
                exp.Responsabilidades.Trim(),
                fechaDesde,
                fechaHasta,
                esActual
            ));
        }

        if (campos.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos });

        bool carnetRepetido = await _db.Alumnos
            .AnyAsync(a => a.IdAlumno != alumno.IdAlumno && a.Carnet.ToUpper() == carnetLimpio);

        if (carnetRepetido)
        {
            return Conflict(new
            {
                mensaje = "Ese carnet ya está registrado.",
                campos = new Dictionary<string, string> { ["carnet"] = "Ese carnet ya está registrado." }
            });
        }

        DatosRegistro reg = alumno.IdUsuarioNavigation.DatosRegistro!;
        reg.Nombres = req.Nombres.Trim();
        reg.Apellidos = req.Apellidos.Trim();
        reg.Telefono = req.Telefono.Trim();

        alumno.IdSituacion = situacion!.IdSituacion;
        alumno.Carnet = carnetLimpio;
        alumno.AnioEgresoGraduacion = anioGuardado;
        alumno.IdAreaEsp = idAreaEsp;

        List<Habilidad> habilidades = await _ofertaService.ObtenerOCrearHabilidadesAsync(_db, req.Habilidades);
        alumno.IdHabilidads.Clear();
        foreach (Habilidad h in habilidades)
            alumno.IdHabilidads.Add(h);

        _db.ExperienciaLaborals.RemoveRange(alumno.ExperienciaLaborals);
        foreach (var exp in experienciasPreparadas)
        {
            _db.ExperienciaLaborals.Add(new ExperienciaLaboral
            {
                IdAlumno = alumno.IdAlumno,
                Cargo = exp.Cargo,
                Empresa = exp.Empresa,
                IdSector = exp.IdSector,
                Responsabilidades = exp.Resp,
                FechaInicio = exp.Desde,
                FechaFin = exp.Hasta,
                EsActual = exp.EsActual
            });
        }

        ConsentimientoVisibilidad? vigente = alumno.ConsentimientoVisibilidads
            .FirstOrDefault(c => c.FechaRevocado == null);

        if (req.Visible && vigente == null)
        {
            _db.ConsentimientoVisibilidads.Add(new ConsentimientoVisibilidad
            {
                IdAlumno = alumno.IdAlumno,
                FechaOtorgado = Validaciones.AhoraSinZona(),
                FechaRevocado = null
            });
        }
        else if (!req.Visible && vigente != null)
        {
            vigente.FechaRevocado = Validaciones.AhoraSinZona();
        }

        await _db.SaveChangesAsync();

        Alumno actualizado = await QueryAlumnoPerfil(asNoTracking: true)
            .FirstAsync(a => a.IdUsuario == userId);

        return Ok(MapearPerfilAlumno(actualizado));
    }

    [HttpPut("foto")]
    public async Task<ActionResult<FotoResponse>> SubirFoto(IFormFile? archivo)
    {
        int userId = ObtenerIdUsuario();
        (bool ok, string? url, string? error) = await _archivoService.GuardarImagenAsync(archivo, "fotos", userId);

        if (!ok)
        {
            return BadRequest(new
            {
                mensaje = error,
                campos = new Dictionary<string, string> { ["archivo"] = error! }
            });
        }

        DatosRegistro? reg = await _db.DatosRegistros.FirstOrDefaultAsync(d => d.IdUsuario == userId);
        if (reg == null)
            return NotFound(new { mensaje = "Registro no encontrado." });

        reg.FotoUrl = url;
        await _db.SaveChangesAsync();

        return Ok(new FotoResponse { FotoUrl = url! });
    }

    [HttpDelete("foto")]
    public async Task<IActionResult> EliminarFoto()
    {
        int userId = ObtenerIdUsuario();
        DatosRegistro? reg = await _db.DatosRegistros.FirstOrDefaultAsync(d => d.IdUsuario == userId);
        if (reg == null)
            return NotFound(new { mensaje = "Registro no encontrado." });

        _archivoService.EliminarImagen("fotos", userId);
        reg.FotoUrl = null;
        await _db.SaveChangesAsync();

        return Ok(new { fotoUrl = "" });
    }

    [HttpGet("ofertas")]
    public async Task<ActionResult<List<OfertaDto>>> ObtenerOfertasVisibles()
    {
        DateOnly hoy = Validaciones.HoyLocal();

        List<OfertaLaboral> ofertas = await _db.OfertaLaborals
            .AsNoTracking()
            .Include(o => o.IdSectorNavigation)
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .Include(o => o.IdEmpresaNavigation)
                .ThenInclude(e => e!.IdUsuarioNavigation)
                    .ThenInclude(u => u.DatosRegistro)
            .Where(o => o.IdEstadoOfertaNavigation.Nombre == Constantes.OfertaPublicada
                        && o.FechaVencimiento >= hoy
                        && o.PuestosVacantes > o.PuestosCubiertos)
            .OrderByDescending(o => o.FechaPublicacion)
            .ThenByDescending(o => o.IdOferta)
            .ToListAsync();

        return Ok(ofertas.Select(o => OfertaService.MapearOferta(o)).ToList());
    }

    [HttpGet("ofertas/{id:int}")]
    public async Task<ActionResult<OfertaDto>> ObtenerOfertaVisible(int id)
    {
        DateOnly hoy = Validaciones.HoyLocal();

        OfertaLaboral? oferta = await _db.OfertaLaborals
            .AsNoTracking()
            .Include(o => o.IdSectorNavigation)
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .Include(o => o.IdEmpresaNavigation)
                .ThenInclude(e => e!.IdUsuarioNavigation)
                    .ThenInclude(u => u.DatosRegistro)
            .FirstOrDefaultAsync(o => o.IdOferta == id
                                      && o.IdEstadoOfertaNavigation.Nombre == Constantes.OfertaPublicada
                                      && o.FechaVencimiento >= hoy
                                      && o.PuestosVacantes > o.PuestosCubiertos);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no disponible." });

        return Ok(OfertaService.MapearOferta(oferta));
    }

    [HttpGet("postulaciones")]
    public async Task<ActionResult<List<PostulacionDto>>> ObtenerMisPostulaciones()
    {
        int userId = ObtenerIdUsuario();
        Alumno? alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.IdUsuario == userId);
        if (alumno == null)
            return NotFound(new { mensaje = "Alumno no encontrado." });

        List<Postulacion> lista = await QueryPostulacionesCompletas()
            .Where(p => p.IdAlumno == alumno.IdAlumno)
            .OrderByDescending(p => p.FechaPostulacion)
            .ToListAsync();

        return Ok(lista.Select(OfertaService.MapearPostulacion).ToList());
    }

    [HttpPost("postulaciones")]
    public async Task<ActionResult<PostulacionDto>> Aplicar([FromBody] AplicarRequest req)
    {
        int userId = ObtenerIdUsuario();

        Alumno? alumno = await _db.Alumnos
            .AsNoTracking()
            .Include(a => a.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .Include(a => a.IdSituacionNavigation)
            .FirstOrDefaultAsync(a => a.IdUsuario == userId);

        if (alumno == null)
            return NotFound(new { mensaje = "Alumno no encontrado." });

        if (!Validaciones.PerfilAlumnoCompleto(alumno, alumno.IdUsuarioNavigation.DatosRegistro!, alumno.IdSituacionNavigation.Nombre))
            return Conflict(new { mensaje = "Completa tu perfil antes de postularte a una oferta." });

        DateOnly hoy = Validaciones.HoyLocal();
        OfertaLaboral? oferta = await _db.OfertaLaborals
            .AsNoTracking()
            .Include(o => o.IdEstadoOfertaNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == req.IdOferta);

        if (oferta == null ||
            oferta.IdEstadoOfertaNavigation.Nombre != Constantes.OfertaPublicada ||
            oferta.FechaVencimiento < hoy ||
            oferta.PuestosVacantes <= oferta.PuestosCubiertos)
        {
            return Conflict(new { mensaje = "La oferta no está disponible para postulaciones." });
        }

        EstadoPostulacion recibida = await _db.EstadoPostulacions
            .FirstAsync(e => e.Nombre == Constantes.PostulacionRecibida);

        Postulacion? existente = await _db.Postulacions
            .Include(p => p.IdEstadoPostulacionNavigation)
            .FirstOrDefaultAsync(p => p.IdOferta == req.IdOferta && p.IdAlumno == alumno.IdAlumno);

        int idPostulacion;

        if (existente != null)
        {
            if (existente.IdEstadoPostulacionNavigation.Nombre != Constantes.PostulacionRetirada)
                return Conflict(new { mensaje = "Ya te postulaste a esta oferta." });

            await using var tx = await _db.Database.BeginTransactionAsync();
            _db.Postulacions.Remove(existente);
            await _db.SaveChangesAsync();

            var nueva = new Postulacion
            {
                IdOferta = req.IdOferta,
                IdAlumno = alumno.IdAlumno,
                IdEstadoPostulacion = recibida.IdEstadoPostulacion
            };
            _db.Postulacions.Add(nueva);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            idPostulacion = nueva.IdPostulacion;
        }
        else
        {
            var nueva = new Postulacion
            {
                IdOferta = req.IdOferta,
                IdAlumno = alumno.IdAlumno,
                IdEstadoPostulacion = recibida.IdEstadoPostulacion
            };
            _db.Postulacions.Add(nueva);
            await _db.SaveChangesAsync();
            idPostulacion = nueva.IdPostulacion;
        }

        Postulacion creada = await QueryPostulacionesCompletas()
            .FirstAsync(p => p.IdPostulacion == idPostulacion);

        return StatusCode(201, OfertaService.MapearPostulacion(creada));
    }

    [HttpPost("postulaciones/{id:int}/retirar")]
    public async Task<ActionResult<PostulacionDto>> RetirarPostulacion(int id)
    {
        int userId = ObtenerIdUsuario();
        Alumno? alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.IdUsuario == userId);
        if (alumno == null)
            return NotFound(new { mensaje = "Alumno no encontrado." });

        Postulacion? postulacion = await _db.Postulacions
            .Include(p => p.IdEstadoPostulacionNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacion == id);

        if (postulacion == null)
            return NotFound(new { mensaje = "Postulación no encontrada." });

        if (postulacion.IdAlumno != alumno.IdAlumno)
            return Forbid();

        string estadoActual = postulacion.IdEstadoPostulacionNavigation.Nombre;
        if (estadoActual != Constantes.PostulacionRecibida && estadoActual != Constantes.PostulacionEnRevision)
            return Conflict(new { mensaje = "Solo puedes retirar postulaciones en estado Recibida o En revisión." });

        EstadoPostulacion retirada = await _db.EstadoPostulacions
            .FirstAsync(e => e.Nombre == Constantes.PostulacionRetirada);

        postulacion.IdEstadoPostulacion = retirada.IdEstadoPostulacion;
        await _db.SaveChangesAsync();

        Postulacion actualizada = await QueryPostulacionesCompletas()
            .FirstAsync(p => p.IdPostulacion == id);

        return Ok(OfertaService.MapearPostulacion(actualizada));
    }

    private IQueryable<Alumno> QueryAlumnoPerfil(bool asNoTracking)
    {
        IQueryable<Alumno> q = _db.Alumnos;
        if (asNoTracking) q = q.AsNoTracking();

        return q
            .Include(a => a.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .Include(a => a.IdSituacionNavigation)
            .Include(a => a.IdAreaEspNavigation)
            .Include(a => a.IdHabilidads)
            .Include(a => a.ExperienciaLaborals)
                .ThenInclude(e => e.IdSectorNavigation)
            .Include(a => a.ConsentimientoVisibilidads);
    }

    private IQueryable<Postulacion> QueryPostulacionesCompletas()
    {
        return _db.Postulacions
            .AsNoTracking()
            .Include(p => p.IdEstadoPostulacionNavigation)
            .Include(p => p.IdOfertaNavigation)
                .ThenInclude(o => o.IdEmpresaNavigation)
            .Include(p => p.IdAlumnoNavigation)
                .ThenInclude(a => a.IdUsuarioNavigation)
                    .ThenInclude(u => u.DatosRegistro)
            .Include(p => p.IdAlumnoNavigation)
                .ThenInclude(a => a.IdSituacionNavigation)
            .Include(p => p.IdAlumnoNavigation)
                .ThenInclude(a => a.IdAreaEspNavigation)
            .Include(p => p.IdAlumnoNavigation)
                .ThenInclude(a => a.IdHabilidads)
            .Include(p => p.IdAlumnoNavigation)
                .ThenInclude(a => a.ExperienciaLaborals)
                    .ThenInclude(e => e.IdSectorNavigation);
    }

    private static PerfilAlumnoDto MapearPerfilAlumno(Alumno a)
    {
        DatosRegistro reg = a.IdUsuarioNavigation.DatosRegistro!;
        ConsentimientoVisibilidad? vigente = a.ConsentimientoVisibilidads
            .FirstOrDefault(c => c.FechaRevocado == null);

        return new PerfilAlumnoDto
        {
            Situacion = a.IdSituacionNavigation.Nombre,
            Nombres = reg.Nombres,
            Apellidos = reg.Apellidos,
            Telefono = reg.Telefono,
            Carnet = a.Carnet,
            AreaEsp = a.IdAreaEspNavigation?.Nombre ?? "",
            Anio = a.AnioEgresoGraduacion?.ToString() ?? "",
            Habilidades = a.IdHabilidads.Select(h => h.Nombre).OrderBy(n => n).ToList(),
            Experiencias = a.ExperienciaLaborals
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
            Visible = vigente != null,
            VisibleDesde = vigente != null ? vigente.FechaOtorgado.ToString("yyyy-MM-dd") : "",
            Correo = a.IdUsuarioNavigation.Correo,
            FotoUrl = reg.FotoUrl ?? ""
        };
    }
}
