namespace PortalGraduados.Api.Helpers;

public static class Constantes
{
    public const string RolAdmin = "Administrador";
    public const string RolAlumno = "Alumno";
    public const string RolEmpresa = "Empresa";

    public const string CuentaPendiente = "Pendiente";
    public const string CuentaActiva = "Activa";
    public const string CuentaSuspendida = "Suspendida";
    public const string CuentaRechazada = "Rechazada";

    public const string SolicitudPendiente = "Pendiente";
    public const string SolicitudAprobada = "Aprobada";
    public const string SolicitudRechazada = "Rechazada";

    public const string OfertaBorrador = "Borrador";
    public const string OfertaPublicada = "Publicada";
    public const string OfertaCubierta = "Cubierta";
    public const string OfertaVencida = "Vencida";
    public const string OfertaCerrada = "Cerrada";

    public const string PostulacionRecibida = "Recibida";
    public const string PostulacionEnRevision = "En revisión";
    public const string PostulacionSeleccionado = "Seleccionado";
    public const string PostulacionDescartado = "Descartado";
    public const string PostulacionRetirada = "Retirada";

    public static readonly string[] Departamentos =
    {
        "Ahuachapán", "Santa Ana", "Sonsonate", "Chalatenango", "La Libertad",
        "San Salvador", "Cuscatlán", "La Paz", "Cabañas", "San Vicente",
        "Usulután", "San Miguel", "Morazán", "La Unión"
    };

    public static readonly string[] TiposEmpleo =
    {
        "Tiempo completo", "Medio tiempo", "Pasantía", "Por proyecto"
    };

    public static readonly string[] NivelesExperiencia =
    {
        "Sin experiencia", "Menos de un año", "1 a 2 años", "3 a 5 años", "Más de 5 años"
    };

    public static readonly string[] Vehiculos =
    {
        "Indiferente", "Motocicleta", "Automóvil"
    };

    public static readonly string[] Modalidades =
    {
        "Presencial", "Remoto", "Híbrido"
    };
}
