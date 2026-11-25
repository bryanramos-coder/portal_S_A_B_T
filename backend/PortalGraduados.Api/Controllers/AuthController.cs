using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Dtos;
using PortalGraduados.Api.Helpers;
using PortalGraduados.Api.Models;
using PortalGraduados.Api.Services;

namespace PortalGraduados.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly PortalDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(PortalDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Correo) || string.IsNullOrEmpty(req.Clave))
        {
            return Ok(new LoginResponse { Estado = "incorrecto" });
        }

        string correoNormalizado = req.Correo.Trim().ToLowerInvariant();

        Usuario? usuario = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .Include(u => u.IdEstadoCuentaNavigation)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNormalizado);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(req.Clave, usuario.ContraseniaHash))
        {
            return Ok(new LoginResponse { Estado = "incorrecto" });
        }

        string estadoCuenta = usuario.IdEstadoCuentaNavigation.Nombre;
        if (estadoCuenta == Constantes.CuentaPendiente)
            return Ok(new LoginResponse { Estado = "pendiente" });
        if (estadoCuenta == Constantes.CuentaSuspendida)
            return Ok(new LoginResponse { Estado = "suspendida" });
        if (estadoCuenta == Constantes.CuentaRechazada)
            return Ok(new LoginResponse { Estado = "rechazada" });

        string rolDb = usuario.IdRolNavigation.Nombre;
        string rolFront = rolDb switch
        {
            Constantes.RolAdmin => "admin",
            Constantes.RolEmpresa => "empresa",
            _ => "alumno"
        };

        string token = _tokenService.CrearToken(usuario, rolDb);

        return Ok(new LoginResponse
        {
            Estado = "ok",
            Rol = rolFront,
            Token = token
        });
    }

    [HttpPost("registro/alumno")]
    public async Task<IActionResult> RegistrarAlumno([FromBody] RegistroAlumnoRequest req)
    {
        var campos = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(req.Nombre) || req.Nombre.Trim().Length > 100)
            campos["nombre"] = "Escribe tu nombre.";

        if (string.IsNullOrWhiteSpace(req.Apellido) || req.Apellido.Trim().Length > 100)
            campos["apellido"] = "Escribe tu apellido.";

        if (!Validaciones.EsCorreoValido(req.Correo))
            campos["correo"] = "Escribe un correo válido.";

        if (!Validaciones.EsTelefonoValido(req.Telefono))
            campos["telefono"] = "Escribe un teléfono válido (de 8 a 15 dígitos).";

        string carnetLimpio = (req.Carnet ?? "").Trim().ToUpperInvariant();
        if (!Validaciones.EsCarnetValido(carnetLimpio))
            campos["carnet"] = "El carnet debe iniciar con dos letras seguidas de 3 a 6 números (Ej: RC23011).";

        string sitNorm = (req.Situacion ?? "").Trim().ToLowerInvariant();
        int? idSituacion = sitNorm switch
        {
            "quinto" or "quinto año" => 1,
            "egresado" => 2,
            "graduado" => 3,
            _ => null
        };

        if (idSituacion == null)
            campos["situacion"] = "Selecciona tu situación académica.";

        short? anioGuardado = null;
        if (idSituacion == 2 || idSituacion == 3)
        {
            if (!Validaciones.EsAnioValido(req.Anio, out short anioParsed))
                campos["anio"] = "Escribe un año entre 1960 y 2100.";
            else
                anioGuardado = anioParsed;
        }

        if (string.IsNullOrEmpty(req.Clave) || req.Clave.Length < 8)
            campos["clave"] = "La contraseña debe tener al menos 8 caracteres.";

        if (campos.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos });

        string correoLimpio = req.Correo.Trim();
        string correoLower = correoLimpio.ToLowerInvariant();

        bool correoExiste = await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoLower);
        if (correoExiste)
        {
            return Conflict(new
            {
                mensaje = "No se pudo completar el registro.",
                campos = new Dictionary<string, string> { ["correo"] = "Ese correo ya está registrado." }
            });
        }

        bool carnetExiste = await _db.Alumnos.AnyAsync(a => a.Carnet.ToUpper() == carnetLimpio);
        if (carnetExiste)
        {
            return Conflict(new
            {
                mensaje = "No se pudo completar el registro.",
                campos = new Dictionary<string, string> { ["carnet"] = "Ese carnet ya está registrado." }
            });
        }

        Rol rolAlumno = await _db.Rols.FirstAsync(r => r.Nombre == Constantes.RolAlumno);
        EstadoCuentum cuentaPendiente = await _db.EstadoCuenta.FirstAsync(e => e.Nombre == Constantes.CuentaPendiente);
        EstadoSolicitud solicitudPendiente = await _db.EstadoSolicituds.FirstAsync(e => e.Nombre == Constantes.SolicitudPendiente);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var usuario = new Usuario
        {
            Correo = correoLimpio,
            ContraseniaHash = BCrypt.Net.BCrypt.HashPassword(req.Clave),
            IdRol = rolAlumno.IdRol,
            IdEstadoCuenta = cuentaPendiente.IdEstadoCuenta
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _db.DatosRegistros.Add(new DatosRegistro
        {
            IdUsuario = usuario.IdUsuario,
            Nombres = req.Nombre.Trim(),
            Apellidos = req.Apellido.Trim(),
            Telefono = req.Telefono.Trim()
        });

        _db.Alumnos.Add(new Alumno
        {
            IdUsuario = usuario.IdUsuario,
            IdSituacion = idSituacion!.Value,
            Carnet = carnetLimpio,
            AnioEgresoGraduacion = anioGuardado
        });

        _db.SolicitudCuenta.Add(new SolicitudCuentum
        {
            IdUsuario = usuario.IdUsuario,
            IdEstadoSolicitud = solicitudPendiente.IdEstadoSolicitud,
            DeclaracionVeracidad = null
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return StatusCode(201, new { mensaje = "Solicitud de cuenta enviada." });
    }

    [HttpPost("registro/empresa")]
    public async Task<IActionResult> RegistrarEmpresa([FromBody] RegistroEmpresaRequest req)
    {
        var campos = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(req.Empresa) || req.Empresa.Trim().Length > 150)
            campos["empresa"] = "Escribe el nombre de la empresa.";

        if (!Validaciones.EsCorreoValido(req.Correo))
            campos["correo"] = "Escribe un correo válido.";

        if (string.IsNullOrWhiteSpace(req.ContactoNombres) || req.ContactoNombres.Trim().Length > 100)
            campos["contactoNombres"] = "Escribe los nombres de la persona de contacto.";

        if (string.IsNullOrWhiteSpace(req.ContactoApellidos) || req.ContactoApellidos.Trim().Length > 100)
            campos["contactoApellidos"] = "Escribe los apellidos de la persona de contacto.";

        if (!Validaciones.EsTelefonoValido(req.Telefono))
            campos["telefono"] = "Escribe un teléfono válido (de 8 a 15 dígitos).";

        if (!string.IsNullOrWhiteSpace(req.Web) && req.Web.Trim().Length > 200)
            campos["web"] = "El sitio web no puede pasar de 200 caracteres.";

        if (!Validaciones.EsClaveEmpresaValida(req.Clave))
            campos["clave"] = "La contraseña debe tener al menos 8 caracteres, incluyendo una letra y un número.";

        if (!req.Terminos)
            campos["terminos"] = "Debes aceptar la declaración de veracidad.";

        if (campos.Count > 0)
            return BadRequest(new { mensaje = "Revisa los campos marcados.", campos });

        string correoLimpio = req.Correo.Trim();
        string correoLower = correoLimpio.ToLowerInvariant();

        bool correoExiste = await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoLower);
        if (correoExiste)
        {
            return Conflict(new
            {
                mensaje = "No se pudo completar el registro.",
                campos = new Dictionary<string, string> { ["correo"] = "Ese correo ya está registrado." }
            });
        }

        Rol rolEmpresa = await _db.Rols.FirstAsync(r => r.Nombre == Constantes.RolEmpresa);
        EstadoCuentum cuentaPendiente = await _db.EstadoCuenta.FirstAsync(e => e.Nombre == Constantes.CuentaPendiente);
        EstadoSolicitud solicitudPendiente = await _db.EstadoSolicituds.FirstAsync(e => e.Nombre == Constantes.SolicitudPendiente);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var usuario = new Usuario
        {
            Correo = correoLimpio,
            ContraseniaHash = BCrypt.Net.BCrypt.HashPassword(req.Clave),
            IdRol = rolEmpresa.IdRol,
            IdEstadoCuenta = cuentaPendiente.IdEstadoCuenta
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _db.DatosRegistros.Add(new DatosRegistro
        {
            IdUsuario = usuario.IdUsuario,
            Nombres = req.ContactoNombres.Trim(),
            Apellidos = req.ContactoApellidos.Trim(),
            Telefono = req.Telefono.Trim()
        });

        _db.Empresas.Add(new Empresa
        {
            IdUsuario = usuario.IdUsuario,
            NombreEmpresa = req.Empresa.Trim(),
            SitioWeb = Validaciones.LimpiarNulo(req.Web)
        });

        _db.SolicitudCuenta.Add(new SolicitudCuentum
        {
            IdUsuario = usuario.IdUsuario,
            IdEstadoSolicitud = solicitudPendiente.IdEstadoSolicitud,
            DeclaracionVeracidad = req.Terminos
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return StatusCode(201, new { mensaje = "Solicitud de cuenta enviada." });
    }
}
