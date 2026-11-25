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
[Route("api/empresa")]
[Authorize(Roles = Constantes.RolEmpresa)]
public class EmpresaController : ControllerBase
{
    private readonly PortalDbContext _db;
    private readonly OfertaService _ofertaService;
    private readonly ArchivoService _archivoService;

    public EmpresaController(PortalDbContext db, OfertaService ofertaService, ArchivoService archivoService)
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
    public async Task<ActionResult<PerfilEmpresaDto>> ObtenerPerfil()
    {
        int userId = ObtenerIdUsuario();

        Empresa? empresa = await _db.Empresas
            .AsNoTracking()
            .Include(e => e.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .FirstOrDefaultAsync(e => e.IdUsuario == userId);

        if (empresa == null)
            return NotFound(new { mensaje = "Perfil de empresa no encontrado." });

        DatosRegistro reg = empresa.IdUsuarioNavigation.DatosRegistro!;

        return Ok(new PerfilEmpresaDto
        {
            NombreEmpresa = empresa.NombreEmpresa,
            Nit = empresa.Nit ?? "",
            Descripcion = empresa.Descripcion ?? "",
            SitioWeb = empresa.SitioWeb ?? "",
            ContactoNombres = reg.Nombres,
            ContactoApellidos = reg.Apellidos,
            Telefono = reg.Telefono,
            CargoContacto = reg.Cargo ?? "",
            Correo = empresa.IdUsuarioNavigation.Correo,
            LogoUrl = reg.FotoUrl ?? ""
        });
    }

    [HttpPut("perfil")]
    public async Task<ActionResult<PerfilEmpresaDto>> ActualizarPerfil([FromBody] ActualizarPerfilEmpresaRequest req)
    {
        var campos = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(req.NombreEmpresa) || req.NombreEmpresa.Trim().Length > 150)
            campos["nombreEmpresa"] = "Escribe el nombre de la empresa.";

        if (!string.IsNullOrWhiteSpace(req.Nit) && !Validaciones.EsNitValido(req.Nit))
            campos["nit"] = "El NIT debe tener 14 dígitos.";

        if (!string.IsNullOrWhiteSpace(req.SitioWeb) && req.SitioWeb.Trim().Length > 200)
            campos["sitioWeb"] = "El sitio web no puede pasar de 200 caracteres.";

        if (string.IsNullOrWhiteSpace(req.ContactoNombres) || req.ContactoNombres.Trim().Length > 100)
            campos["contactoNombres"] = "Escribe los nombres del contacto.";

        if (string.IsNullOrWhiteSpace(req.ContactoApellidos) || req.ContactoApellidos.Trim().Length > 100)
            campos["contactoApellidos"] = "Escribe los apellidos del contacto.";

        if (!Validaciones.EsTelefonoValido(req.Telefono))
            campos["telefono"] = "Escribe un teléfono válido (de 8 a 15 dígitos).";

        if (!string.IsNullOrWhiteSpace(req.CargoContacto) && req.CargoContacto.Trim().Length > 100)
            campos["cargoContacto"] = "El cargo no puede pasar de 100 caracteres.";

        if (campos.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos });

        int userId = ObtenerIdUsuario();

        Empresa? empresa = await _db.Empresas
            .Include(e => e.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .FirstOrDefaultAsync(e => e.IdUsuario == userId);

        if (empresa == null)
            return NotFound(new { mensaje = "Perfil de empresa no encontrado." });

        DatosRegistro reg = empresa.IdUsuarioNavigation.DatosRegistro!;

        empresa.NombreEmpresa = req.NombreEmpresa.Trim();
        empresa.Nit = Validaciones.LimpiarNulo(req.Nit);
        empresa.Descripcion = Validaciones.LimpiarNulo(req.Descripcion);
        empresa.SitioWeb = Validaciones.LimpiarNulo(req.SitioWeb);

        reg.Nombres = req.ContactoNombres.Trim();
        reg.Apellidos = req.ContactoApellidos.Trim();
        reg.Telefono = req.Telefono.Trim();
        reg.Cargo = Validaciones.LimpiarNulo(req.CargoContacto);

        await _db.SaveChangesAsync();

        return Ok(new PerfilEmpresaDto
        {
            NombreEmpresa = empresa.NombreEmpresa,
            Nit = empresa.Nit ?? "",
            Descripcion = empresa.Descripcion ?? "",
            SitioWeb = empresa.SitioWeb ?? "",
            ContactoNombres = reg.Nombres,
            ContactoApellidos = reg.Apellidos,
            Telefono = reg.Telefono,
            CargoContacto = reg.Cargo ?? "",
            Correo = empresa.IdUsuarioNavigation.Correo,
            LogoUrl = reg.FotoUrl ?? ""
        });
    }

    [HttpPut("logo")]
    public async Task<ActionResult<LogoResponse>> SubirLogo(IFormFile? archivo)
    {
        int userId = ObtenerIdUsuario();
        (bool ok, string? url, string? error) = await _archivoService.GuardarImagenAsync(archivo, "logos", userId);

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

        return Ok(new LogoResponse { LogoUrl = url! });
    }

    [HttpDelete("logo")]
    public async Task<IActionResult> EliminarLogo()
    {
        int userId = ObtenerIdUsuario();
        DatosRegistro? reg = await _db.DatosRegistros.FirstOrDefaultAsync(d => d.IdUsuario == userId);
        if (reg == null)
            return NotFound(new { mensaje = "Registro no encontrado." });

        _archivoService.EliminarImagen("logos", userId);
        reg.FotoUrl = null;
        await _db.SaveChangesAsync();

        return Ok(new { logoUrl = "" });
    }

    [HttpGet("ofertas")]
    public async Task<ActionResult<List<OfertaDto>>> ObtenerOfertas()
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        List<OfertaLaboral> ofertas = await QueryOfertasCompletas(incluirPostulaciones: true)
            .Where(o => o.IdEmpresa == empresa.IdEmpresa)
            .OrderByDescending(o => o.IdOferta)
            .ToListAsync();

        return Ok(ofertas.Select(o => OfertaService.MapearOferta(o, incluirConteoPostulantes: true)).ToList());
    }

    [HttpGet("ofertas/{id:int}")]
    public async Task<ActionResult<OfertaDto>> ObtenerOferta(int id)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        OfertaLaboral? oferta = await QueryOfertasCompletas(incluirPostulaciones: true)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != empresa.IdEmpresa)
            return Forbid();

        return Ok(OfertaService.MapearOferta(oferta, incluirConteoPostulantes: true));
    }

    [HttpPost("ofertas")]
    public async Task<ActionResult<OfertaDto>> CrearOferta([FromBody] FormOfertaRequest req)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        Dictionary<string, string> errores = await _ofertaService.ValidarOfertaAsync(_db, req, esExterna: false, paraPublicar: false);
        if (errores.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos = errores });

        Sector sector = await _db.Sectors.FirstAsync(s => s.Nombre == req.Sector.Trim());
        EstadoOfertum borrador = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaBorrador);
        List<Habilidad> habilidades = await _ofertaService.ObtenerOCrearHabilidadesAsync(_db, req.Habilidades);

        var oferta = new OfertaLaboral
        {
            IdEmpresa = empresa.IdEmpresa,
            EmpresaExterna = null,
            IdPublicador = userId,
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

        OfertaLaboral creada = await QueryOfertasCompletas(incluirPostulaciones: true)
            .FirstAsync(o => o.IdOferta == oferta.IdOferta);

        return StatusCode(201, OfertaService.MapearOferta(creada, incluirConteoPostulantes: true));
    }

    [HttpPut("ofertas/{id:int}")]
    public async Task<ActionResult<OfertaDto>> EditarOferta(int id, [FromBody] FormOfertaRequest req)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != empresa.IdEmpresa)
            return Forbid();

        string estado = oferta.IdEstadoOfertaNavigation.Nombre;
        if (estado != Constantes.OfertaBorrador && estado != Constantes.OfertaPublicada)
            return Conflict(new { mensaje = "Solo se pueden editar ofertas en estado Borrador o Publicada." });

        bool estaPublicada = estado == Constantes.OfertaPublicada;
        Dictionary<string, string> errores = await _ofertaService.ValidarOfertaAsync(_db, req, esExterna: false, paraPublicar: estaPublicada);
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

        OfertaLaboral actualizada = await QueryOfertasCompletas(incluirPostulaciones: true)
            .FirstAsync(o => o.IdOferta == id);

        return Ok(OfertaService.MapearOferta(actualizada, incluirConteoPostulantes: true));
    }

    [HttpPost("ofertas/{id:int}/publicar")]
    public async Task<ActionResult<OfertaDto>> PublicarOferta(int id)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas
            .AsNoTracking()
            .Include(e => e.IdUsuarioNavigation)
                .ThenInclude(u => u.DatosRegistro)
            .FirstOrDefaultAsync(e => e.IdUsuario == userId);

        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != empresa.IdEmpresa)
            return Forbid();

        if (!Validaciones.PerfilEmpresaCompleto(empresa, empresa.IdUsuarioNavigation.DatosRegistro!))
            return Conflict(new { mensaje = "Completa el perfil de tu empresa antes de publicar ofertas." });

        if (oferta.IdEstadoOfertaNavigation.Nombre != Constantes.OfertaBorrador)
            return Conflict(new { mensaje = "Solo se pueden publicar ofertas en estado Borrador." });

        if (oferta.IdHabilidads.Count == 0)
            return Conflict(new { mensaje = "Agrega al menos una habilidad para poder publicar." });

        if (oferta.FechaVencimiento < Validaciones.HoyLocal())
            return Conflict(new { mensaje = "La fecha de expiración debe ser vigente para publicar." });

        EstadoOfertum publicada = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaPublicada);
        oferta.IdEstadoOferta = publicada.IdEstadoOferta;
        await _db.SaveChangesAsync();

        OfertaLaboral actualizada = await QueryOfertasCompletas(incluirPostulaciones: true)
            .FirstAsync(o => o.IdOferta == id);

        return Ok(OfertaService.MapearOferta(actualizada, incluirConteoPostulantes: true));
    }

    [HttpPost("ofertas/{id:int}/cerrar")]
    public async Task<ActionResult<OfertaDto>> CerrarOferta(int id)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        OfertaLaboral? oferta = await _db.OfertaLaborals
            .Include(o => o.IdEstadoOfertaNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null)
            return NotFound(new { mensaje = "Oferta no encontrada." });

        if (oferta.IdEmpresa != empresa.IdEmpresa)
            return Forbid();

        string estado = oferta.IdEstadoOfertaNavigation.Nombre;
        if (estado != Constantes.OfertaPublicada && estado != Constantes.OfertaCubierta)
            return Conflict(new { mensaje = "Solo se pueden cerrar ofertas en estado Publicada o Cubierta." });

        EstadoOfertum cerrada = await _db.EstadoOferta.FirstAsync(e => e.Nombre == Constantes.OfertaCerrada);
        oferta.IdEstadoOferta = cerrada.IdEstadoOferta;
        await _db.SaveChangesAsync();

        OfertaLaboral actualizada = await QueryOfertasCompletas(incluirPostulaciones: true)
            .FirstAsync(o => o.IdOferta == id);

        return Ok(OfertaService.MapearOferta(actualizada, incluirConteoPostulantes: true));
    }

    [HttpGet("postulantes")]
    public async Task<ActionResult<List<PostulacionDto>>> ObtenerPostulantes()
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        List<Postulacion> lista = await QueryPostulacionesCompletas()
            .Where(p => p.IdOfertaNavigation.IdEmpresa == empresa.IdEmpresa)
            .OrderByDescending(p => p.FechaPostulacion)
            .ToListAsync();

        return Ok(lista.Select(OfertaService.MapearPostulacion).ToList());
    }

    [HttpPatch("postulantes/{id:int}/estado")]
    public async Task<ActionResult<PostulacionDto>> CambiarEstadoPostulante(int id, [FromBody] CambiarEstadoPostulacionRequest req)
    {
        int userId = ObtenerIdUsuario();
        Empresa? empresa = await _db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdUsuario == userId);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada." });

        string nuevoEstado = (req.Estado ?? "").Trim();
        if (nuevoEstado != Constantes.PostulacionEnRevision &&
            nuevoEstado != Constantes.PostulacionSeleccionado &&
            nuevoEstado != Constantes.PostulacionDescartado)
        {
            return BadRequest(new { mensaje = "Estado de postulación no válido." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        Postulacion? postulacion = await _db.Postulacions
            .Include(p => p.IdEstadoPostulacionNavigation)
            .Include(p => p.IdOfertaNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacion == id);

        if (postulacion == null)
            return NotFound(new { mensaje = "Postulación no encontrada." });

        if (postulacion.IdOfertaNavigation.IdEmpresa != empresa.IdEmpresa)
            return Forbid();

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM oferta_laboral WHERE id_oferta = {postulacion.IdOferta} FOR UPDATE");

        await _db.Entry(postulacion.IdOfertaNavigation).ReloadAsync();

        string estadoActual = postulacion.IdEstadoPostulacionNavigation.Nombre;
        bool transicionValida =
            (estadoActual == Constantes.PostulacionRecibida &&
             (nuevoEstado == Constantes.PostulacionEnRevision ||
              nuevoEstado == Constantes.PostulacionSeleccionado ||
              nuevoEstado == Constantes.PostulacionDescartado))
            ||
            (estadoActual == Constantes.PostulacionEnRevision &&
             (nuevoEstado == Constantes.PostulacionSeleccionado ||
              nuevoEstado == Constantes.PostulacionDescartado));

        if (!transicionValida)
            return Conflict(new { mensaje = $"No se puede pasar de {estadoActual} a {nuevoEstado}." });

        if (nuevoEstado == Constantes.PostulacionSeleccionado &&
            postulacion.IdOfertaNavigation.PuestosCubiertos >= postulacion.IdOfertaNavigation.PuestosVacantes)
        {
            return Conflict(new { mensaje = "La oferta ya no tiene vacantes disponibles." });
        }

        EstadoPostulacion estadoDestino = await _db.EstadoPostulacions
            .FirstAsync(e => e.Nombre == nuevoEstado);

        postulacion.IdEstadoPostulacion = estadoDestino.IdEstadoPostulacion;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        Postulacion actualizada = await QueryPostulacionesCompletas()
            .FirstAsync(p => p.IdPostulacion == id);

        return Ok(OfertaService.MapearPostulacion(actualizada));
    }

    private IQueryable<OfertaLaboral> QueryOfertasCompletas(bool incluirPostulaciones)
    {
        IQueryable<OfertaLaboral> q = _db.OfertaLaborals
            .AsNoTracking()
            .Include(o => o.IdSectorNavigation)
            .Include(o => o.IdEstadoOfertaNavigation)
            .Include(o => o.IdHabilidads)
            .Include(o => o.IdEmpresaNavigation)
                .ThenInclude(e => e!.IdUsuarioNavigation)
                    .ThenInclude(u => u.DatosRegistro);

        if (incluirPostulaciones)
        {
            q = q.Include(o => o.Postulacions)
                .ThenInclude(p => p.IdEstadoPostulacionNavigation);
        }

        return q;
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
}
