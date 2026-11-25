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
[Route("api/admin")]
[Authorize(Roles = Constantes.RolAdmin)]
public class AdminController : ControllerBase
{
    private readonly PortalDbContext _db;
    private readonly OfertaService _ofertaService;

    public AdminController(PortalDbContext db, OfertaService ofertaService)
    {
        _db = db;
        _ofertaService = ofertaService;
    }

    private int ObtenerIdUsuario()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<List<UsuarioAdminDto>>> ObtenerUsuarios()
    {
        List<Usuario> usuarios = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .Include(u => u.IdEstadoCuentaNavigation)
            .Include(u => u.DatosRegistro)
            .Include(u => u.Alumno)
                .ThenInclude(a => a!.IdSituacionNavigation)
            .Include(u => u.Empresa)
            .OrderByDescending(u => u.IdUsuario)
            .ToListAsync();

        return Ok(usuarios.Select(MapearUsuarioAdmin).ToList());
    }

    [HttpPatch("usuarios/{id:int}/estado")]
    public async Task<ActionResult<UsuarioAdminDto>> CambiarEstadoUsuario(int id, [FromBody] CambiarEstadoUsuarioRequest req)
    {
        int adminId = ObtenerIdUsuario();
        string accion = (req.Accion ?? "").Trim().ToLowerInvariant();

        if (accion != "activar" && accion != "rechazar" && accion != "suspender")
            return BadRequest(new { mensaje = "Acción no válida." });

        Usuario? usuario = await _db.Usuarios
            .Include(u => u.IdEstadoCuentaNavigation)
            .Include(u => u.SolicitudCuentumIdUsuarioNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.IdUsuario == adminId)
            return Conflict(new { mensaje = "No puedes cambiar el estado de tu propia cuenta." });

        string estadoActual = usuario.IdEstadoCuentaNavigation.Nombre;

        if (accion == "activar")
        {
            if (estadoActual == Constantes.CuentaPendiente)
            {
                if (usuario.SolicitudCuentumIdUsuarioNavigation != null)
                {
                    EstadoSolicitud aprobada = await _db.EstadoSolicituds
                        .FirstAsync(s => s.Nombre == Constantes.SolicitudAprobada);

                    usuario.SolicitudCuentumIdUsuarioNavigation.IdEstadoSolicitud = aprobada.IdEstadoSolicitud;
                    usuario.SolicitudCuentumIdUsuarioNavigation.IdRevisor = adminId;
                }
                else
                {
                    EstadoCuentum activa = await _db.EstadoCuenta
                        .FirstAsync(e => e.Nombre == Constantes.CuentaActiva);
                    usuario.IdEstadoCuenta = activa.IdEstadoCuenta;
                }
            }
            else if (estadoActual == Constantes.CuentaSuspendida)
            {
                EstadoCuentum activa = await _db.EstadoCuenta
                    .FirstAsync(e => e.Nombre == Constantes.CuentaActiva);
                usuario.IdEstadoCuenta = activa.IdEstadoCuenta;
            }
            else
            {
                return Conflict(new { mensaje = "No se puede activar una cuenta en este estado." });
            }
        }
        else if (accion == "rechazar")
        {
            if (estadoActual != Constantes.CuentaPendiente)
                return Conflict(new { mensaje = "Solo se pueden rechazar cuentas pendientes." });

            if (usuario.SolicitudCuentumIdUsuarioNavigation != null)
            {
                EstadoSolicitud rechazada = await _db.EstadoSolicituds
                    .FirstAsync(s => s.Nombre == Constantes.SolicitudRechazada);

                usuario.SolicitudCuentumIdUsuarioNavigation.IdEstadoSolicitud = rechazada.IdEstadoSolicitud;
                usuario.SolicitudCuentumIdUsuarioNavigation.IdRevisor = adminId;
            }
            else
            {
                EstadoCuentum rechazada = await _db.EstadoCuenta
                    .FirstAsync(e => e.Nombre == Constantes.CuentaRechazada);
                usuario.IdEstadoCuenta = rechazada.IdEstadoCuenta;
            }
        }
        else
        {
            if (estadoActual != Constantes.CuentaActiva)
                return Conflict(new { mensaje = "Solo se pueden suspender cuentas activas." });

            EstadoCuentum suspendida = await _db.EstadoCuenta
                .FirstAsync(e => e.Nombre == Constantes.CuentaSuspendida);
            usuario.IdEstadoCuenta = suspendida.IdEstadoCuenta;
        }

        await _db.SaveChangesAsync();

        Usuario actualizado = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .Include(u => u.IdEstadoCuentaNavigation)
            .Include(u => u.DatosRegistro)
            .Include(u => u.Alumno)
                .ThenInclude(a => a!.IdSituacionNavigation)
            .Include(u => u.Empresa)
            .FirstAsync(u => u.IdUsuario == id);

        return Ok(MapearUsuarioAdmin(actualizado));
    }

    [HttpGet("ofertas")]
    public async Task<ActionResult<List<OfertaDto>>> ObtenerOfertas()
    {
        List<OfertaLaboral> ofertas = await QueryOfertasCompletas()
            .OrderByDescending(o => o.IdOferta)
            .ToListAsync();

        return Ok(ofertas.Select(o => OfertaService.MapearOferta(o)).ToList());
    }

    [HttpGet("ofertas/{id:int}")]
    public async Task<ActionResult<OfertaDto>> ObtenerOferta(int id)
    {
        OfertaLaboral? oferta = await QueryOfertasCompletas()
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        return Ok(OfertaService.MapearOferta(oferta));
    }

    [HttpPost("ofertas")]
    public async Task<ActionResult<OfertaDto>> CrearOferta([FromBody] FormOfertaRequest req)
    {
        Dictionary<string, string> errores = await _ofertaService.ValidarOfertaAsync(_db, req, esExterna: true, paraPublicar: false);
        if (errores.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos = errores });

        int adminId = ObtenerIdUsuario();
        Sector sector = await _db.Sectors.FirstAsync(s => s.Nombre == req.Sector.Trim());
        EstadoOfertum borrador = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaBorrador);
        List<Habilidad> habilidades = await _ofertaService.ObtenerOCrearHabilidadesAsync(_db, req.Habilidades);

        var oferta = new OfertaLaboral
        {
            IdEmpresa = null,
            EmpresaExterna = req.Empresa.Trim(),
            IdPublicador = adminId,
            IdSector = sector.IdSector,
            IdEstadoOferta = borrador.IdEstadoOferta,
            Titulo = req.Puesto.Trim(),
            Mision = req.Objetivo.Trim(),
            Funciones = req.Funciones.Trim(),
            Requisitos = req.Requisitos.Trim(),
            Competencias = Validaciones.LimpiarNulo(req.Competencias),
            Horarios = Validaciones.LimpiarNulo(req.Horarios),
            Ofrecemos = Validaciones.LimpiarNulo(req.Ofrecemos),
            PuestosVacantes = (short)req.Vacantes,
            PuestosCubiertos = 0,
            NivelExperiencia = req.NivelExperiencia.Trim(),
            Departamento = req.Departamento.Trim(),
            DetalleUbicacion = Validaciones.LimpiarNulo(req.UbicacionDetalle),
            TipoEmpleo = req.Tipo.Trim(),
            Modalidad = Validaciones.LimpiarNulo(req.Modalidad),
            Vehiculo = Validaciones.LimpiarNulo(req.Vehiculo),
            SalarioMin = req.SalarioMin,
            SalarioMax = req.SalarioMax,
            PersonaContacto = Validaciones.LimpiarNulo(req.ContactoNombre),
            CorreoContacto = Validaciones.LimpiarNulo(req.ContactoCorreo),
            Indicaciones = Validaciones.LimpiarNulo(req.Indicaciones),
            FechaVencimiento = DateOnly.ParseExact(req.FechaExpiracion.Trim(), "yyyy-MM-dd")
        };

        foreach (Habilidad h in habilidades)
            oferta.IdHabilidads.Add(h);

        _db.OfertaLaborals.Add(oferta);
        await _db.SaveChangesAsync();

        OfertaLaboral creada = await QueryOfertasCompletas().FirstAsync(o => o.IdOferta == oferta.IdOferta);
        return StatusCode(201, OfertaService.MapearOferta(creada));
    }

    [HttpPut("ofertas/{id:int}")]
    public async Task<ActionResult<OfertaDto>> EditarOferta(int id, [FromBody] FormOfertaRequest req)
    {
        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdPublicadorNavigation)
                .ThenInclude(u => u.IdRolNavigation)
            .Include(o => o.IdHabilidads)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != null || oferta.IdPublicadorNavigation.IdRolNavigation.Nombre != Constantes.RolAdmin)
            return Forbid();

        string estado = oferta.IdEstadoOfertaNavigation.Nombre;
        if (estado != Constantes.OfertaBorrador && estado != Constantes.OfertaPublicada)
            return Conflict(new { mensaje = "Solo se pueden editar ofertas en estado Borrador o Publicada." });

        bool estaPublicada = estado == Constantes.OfertaPublicada;
        Dictionary<string, string> errores = await _ofertaService.ValidarOfertaAsync(_db, req, esExterna: true, paraPublicar: estaPublicada);
        if (errores.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos = errores });

        if (req.Vacantes < oferta.PuestosCubiertos)
        {
            return Conflict(new
            {
                mensaje = "Las vacantes no pueden ser menores que los puestos ya cubiertos.",
                campos = new Dictionary<string, string> { ["vacantes"] = "No puede ser menor a los puestos ya cubiertos." }
            });
        }

        Sector sector = await _db.Sectors.FirstAsync(s => s.Nombre == req.Sector.Trim());
        List<Habilidad> habilidades = await _ofertaService.ObtenerOCrearHabilidadesAsync(_db, req.Habilidades);

        oferta.EmpresaExterna = req.Empresa.Trim();
        oferta.IdSector = sector.IdSector;
        oferta.Titulo = req.Puesto.Trim();
        oferta.Mision = req.Objetivo.Trim();
        oferta.Funciones = req.Funciones.Trim();
        oferta.Requisitos = req.Requisitos.Trim();
        oferta.Competencias = Validaciones.LimpiarNulo(req.Competencias);
        oferta.Horarios = Validaciones.LimpiarNulo(req.Horarios);
        oferta.Ofrecemos = Validaciones.LimpiarNulo(req.Ofrecemos);
        oferta.PuestosVacantes = (short)req.Vacantes;
        oferta.NivelExperiencia = req.NivelExperiencia.Trim();
        oferta.Departamento = req.Departamento.Trim();
        oferta.DetalleUbicacion = Validaciones.LimpiarNulo(req.UbicacionDetalle);
        oferta.TipoEmpleo = req.Tipo.Trim();
        oferta.Modalidad = Validaciones.LimpiarNulo(req.Modalidad);
        oferta.Vehiculo = Validaciones.LimpiarNulo(req.Vehiculo);
        oferta.SalarioMin = req.SalarioMin;
        oferta.SalarioMax = req.SalarioMax;
        oferta.PersonaContacto = Validaciones.LimpiarNulo(req.ContactoNombre);
        oferta.CorreoContacto = Validaciones.LimpiarNulo(req.ContactoCorreo);
        oferta.Indicaciones = Validaciones.LimpiarNulo(req.Indicaciones);
        oferta.FechaVencimiento = DateOnly.ParseExact(req.FechaExpiracion.Trim(), "yyyy-MM-dd");

        oferta.IdHabilidads.Clear();
        foreach (Habilidad h in habilidades)
            oferta.IdHabilidads.Add(h);

        await _db.SaveChangesAsync();

        OfertaLaboral actualizada = await QueryOfertasCompletas().FirstAsync(o => o.IdOferta == id);
        return Ok(OfertaService.MapearOferta(actualizada));
    }

    [HttpPost("ofertas/{id:int}/publicar")]
    public async Task<ActionResult<OfertaDto>> PublicarOferta(int id)
    {
        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdPublicadorNavigation)
                .ThenInclude(u => u.IdRolNavigation)
            .Include(o => o.IdHabilidads)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != null || oferta.IdPublicadorNavigation.IdRolNavigation.Nombre != Constantes.RolAdmin)
            return Forbid();

        if (oferta.IdEstadoOfertaNavigation.Nombre != Constantes.OfertaBorrador)
            return Conflict(new { mensaje = "Solo se pueden publicar ofertas en estado Borrador." });

        if (oferta.IdHabilidads.Count == 0)
            return Conflict(new { mensaje = "Agrega al menos una habilidad para poder publicar." });

        if (oferta.FechaVencimiento < Validaciones.HoyLocal())
            return Conflict(new { mensaje = "La fecha de expiración debe ser vigente para publicar." });

        EstadoOfertum publicada = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaPublicada);
        oferta.IdEstadoOferta = publicada.IdEstadoOferta;
        await _db.SaveChangesAsync();

        OfertaLaboral actualizada = await QueryOfertasCompletas().FirstAsync(o => o.IdOferta == id);
        return Ok(OfertaService.MapearOferta(actualizada));
    }

    [HttpPost("ofertas/{id:int}/cerrar")]
    public async Task<ActionResult<OfertaDto>> CerrarOferta(int id)
    {
        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdPublicadorNavigation)
                .ThenInclude(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != null || oferta.IdPublicadorNavigation.IdRolNavigation.Nombre != Constantes.RolAdmin)
            return Forbid();

        string estado = oferta.IdEstadoOfertaNavigation.Nombre;
        if (estado != Constantes.OfertaPublicada && estado != Constantes.OfertaCubierta)
            return Conflict(new { mensaje = "Solo se pueden cerrar ofertas en estado Publicada o Cubierta." });

        EstadoOfertum cerrada = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaCerrada);
        oferta.IdEstadoOferta = cerrada.IdEstadoOferta;
        await _db.SaveChangesAsync();

        OfertaLaboral actualizada = await QueryOfertasCompletas().FirstAsync(o => o.IdOferta == id);
        return Ok(OfertaService.MapearOferta(actualizada));
    }

    [HttpGet("seguimiento")]
    public async Task<ActionResult<List<SeguimientoPerfilDto>>> ObtenerSeguimiento()
    {
        List<Alumno> alumnos = await _db.Alumnos
            .AsNoTracking()
            .Include(a => a.IdUsuarioNavigation)
                .ThenInclude(u => u.IdEstadoCuentaNavigation)
            .Include(a => a.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .Include(a => a.IdSituacionNavigation)
            .Include(a => a.IdAreaEspNavigation)
            .Include(a => a.IdHabilidads)
            .Include(a => a.ExperienciaLaborals)
                .ThenInclude(e => e.IdSectorNavigation)
            .Where(a => a.IdUsuarioNavigation.IdEstadoCuentaNavigation.Nombre == Constantes.CuentaActiva
                        && (a.IdSituacionNavigation.Nombre == "Graduado" || a.IdSituacionNavigation.Nombre == "Egresado"))
            .OrderByDescending(a => a.AnioEgresoGraduacion)
            .ThenBy(a => a.IdAlumno)
            .ToListAsync();

        var resultado = new List<SeguimientoPerfilDto>();

        foreach (Alumno a in alumnos)
        {
            DatosRegistro? reg = a.IdUsuarioNavigation.DatosRegistro;
            string nombres = reg?.Nombres ?? "";
            string apellidos = reg?.Apellidos ?? "";

            List<ExperienciaLaboral> ordenadas = a.ExperienciaLaborals
                .OrderByDescending(e => e.FechaInicio)
                .ThenByDescending(e => e.IdExperiencia)
                .ToList();

            ExperienciaLaboral? actual = ordenadas.FirstOrDefault(e => e.EsActual);

            bool completo = reg != null && Validaciones.PerfilAlumnoCompleto(a, reg, a.IdSituacionNavigation.Nombre);

            resultado.Add(new SeguimientoPerfilDto
            {
                Id = a.IdAlumno,
                Nombre = $"{nombres} {apellidos}".Trim(),
                Correo = a.IdUsuarioNavigation.Correo,
                Condicion = a.IdSituacionNavigation.Nombre,
                Anio = a.AnioEgresoGraduacion ?? 0,
                AreaEspecializacion = a.IdAreaEspNavigation?.Nombre ?? "",
                Empleado = actual != null,
                PuestoActual = actual?.Cargo ?? "",
                EmpresaActual = actual?.Empresa ?? "",
                Sector = actual?.IdSectorNavigation?.Nombre ?? "",
                InicioPuestoActual = actual != null ? actual.FechaInicio.ToString("yyyy-MM-dd") : "",
                Tecnologias = a.IdHabilidads.Select(h => h.Nombre).OrderBy(n => n).ToList(),
                PerfilCompleto = completo,
                Historial = ordenadas.Select(e => new SeguimientoHistorialDto
                {
                    Cargo = e.Cargo,
                    Empresa = e.Empresa,
                    Sector = e.IdSectorNavigation?.Nombre ?? "",
                    Desde = e.FechaInicio.ToString("yyyy-MM-dd"),
                    Hasta = e.EsActual ? null : e.FechaFin?.ToString("yyyy-MM-dd")
                }).ToList()
            });
        }

        return Ok(resultado);
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenAdminDto>> ObtenerResumen()
    {
        int egresadosRegistrados = await _db.Alumnos
            .CountAsync(a => a.IdUsuarioNavigation.IdEstadoCuentaNavigation.Nombre == Constantes.CuentaActiva
                             && (a.IdSituacionNavigation.Nombre == "Graduado" || a.IdSituacionNavigation.Nombre == "Egresado"));

        int alumnosRegistrados = await _db.Alumnos
            .CountAsync(a => a.IdUsuarioNavigation.IdEstadoCuentaNavigation.Nombre == Constantes.CuentaActiva);

        int empresasActivas = await _db.Empresas
            .CountAsync(e => e.IdUsuarioNavigation.IdEstadoCuentaNavigation.Nombre == Constantes.CuentaActiva);

        int ofertasPublicadas = await _db.OfertaLaborals
            .CountAsync(o => o.IdEstadoOfertaNavigation.Nombre == Constantes.OfertaPublicada);

        int totalPost = await _db.Postulacions.CountAsync();
        int enCurso = await _db.Postulacions
            .CountAsync(p => p.IdEstadoPostulacionNavigation.Nombre == Constantes.PostulacionRecibida
                             || p.IdEstadoPostulacionNavigation.Nombre == Constantes.PostulacionEnRevision);
        int seleccionados = await _db.Postulacions
            .CountAsync(p => p.IdEstadoPostulacionNavigation.Nombre == Constantes.PostulacionSeleccionado);

        return Ok(new ResumenAdminDto
        {
            EgresadosRegistrados = egresadosRegistrados,
            EmpresasActivas = empresasActivas,
            OfertasPublicadas = ofertasPublicadas,
            AlumnosRegistrados = alumnosRegistrados,
            Postulaciones = new ResumenPostulacionesDto
            {
                Total = totalPost,
                EnCurso = enCurso,
                Seleccionados = seleccionados
            }
        });
    }

    private IQueryable<OfertaLaboral> QueryOfertasCompletas()
    {
        return _db.OfertaLaborals
            .AsNoTracking()
            .Include(o => o.IdSectorNavigation)
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .Include(o => o.IdEmpresaNavigation)
                .ThenInclude(e => e!.IdUsuarioNavigation)
                    .ThenInclude(u => u.DatosRegistro);
    }

    private static UsuarioAdminDto MapearUsuarioAdmin(Usuario u)
    {
        string rol = u.IdRolNavigation.Nombre;
        DatosRegistro? reg = u.DatosRegistro;

        string nombre = rol == Constantes.RolEmpresa
            ? (u.Empresa?.NombreEmpresa ?? "")
            : $"{reg?.Nombres ?? ""} {reg?.Apellidos ?? ""}".Trim();

        var dto = new UsuarioAdminDto
        {
            Id = u.IdUsuario,
            Nombre = nombre,
            Correo = u.Correo,
            Tipo = rol,
            Estado = u.IdEstadoCuentaNavigation.Nombre,
            FechaRegistro = reg != null ? reg.FechaRegistro.ToString("yyyy-MM-dd") : "",
            Telefono = reg?.Telefono ?? ""
        };

        if (rol == Constantes.RolAlumno && u.Alumno != null)
        {
            dto.Carnet = u.Alumno.Carnet;
            dto.Situacion = u.Alumno.IdSituacionNavigation.Nombre;
            dto.Anio = u.Alumno.AnioEgresoGraduacion;
        }
        else if (rol == Constantes.RolEmpresa && u.Empresa != null)
        {
            dto.ContactoNombres = reg?.Nombres ?? "";
            dto.ContactoApellidos = reg?.Apellidos ?? "";
            dto.CargoContacto = reg?.Cargo ?? "";
            dto.SitioWeb = u.Empresa.SitioWeb ?? "";
        }

        return dto;
    }
}
