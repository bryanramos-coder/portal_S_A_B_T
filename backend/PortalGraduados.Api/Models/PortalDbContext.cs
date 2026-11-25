using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PortalGraduados.Api.Models;

public partial class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alumno> Alumnos { get; set; }

    public virtual DbSet<AreaEspecializacion> AreaEspecializacions { get; set; }

    public virtual DbSet<Aviso> Avisos { get; set; }

    public virtual DbSet<ConsentimientoVisibilidad> ConsentimientoVisibilidads { get; set; }

    public virtual DbSet<DatosRegistro> DatosRegistros { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<EstadoCuentum> EstadoCuenta { get; set; }

    public virtual DbSet<EstadoOfertum> EstadoOferta { get; set; }

    public virtual DbSet<EstadoPostulacion> EstadoPostulacions { get; set; }

    public virtual DbSet<EstadoSolicitud> EstadoSolicituds { get; set; }

    public virtual DbSet<ExperienciaLaboral> ExperienciaLaborals { get; set; }

    public virtual DbSet<Habilidad> Habilidads { get; set; }

    public virtual DbSet<OfertaLaboral> OfertaLaborals { get; set; }

    public virtual DbSet<Postulacion> Postulacions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Sector> Sectors { get; set; }

    public virtual DbSet<SituacionAcademica> SituacionAcademicas { get; set; }

    public virtual DbSet<SolicitudCuentum> SolicitudCuenta { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwCandidatosOfertum> VwCandidatosOferta { get; set; }

    public virtual DbSet<VwEmpleabilidadPorAnio> VwEmpleabilidadPorAnios { get; set; }

    public virtual DbSet<VwMisPostulacione> VwMisPostulaciones { get; set; }

    public virtual DbSet<VwOfertasVigente> VwOfertasVigentes { get; set; }

    public virtual DbSet<VwPerfilAlumnoPublico> VwPerfilAlumnoPublicos { get; set; }

    public virtual DbSet<VwResumenOferta> VwResumenOfertas { get; set; }

    public virtual DbSet<VwSeguimientoAlumno> VwSeguimientoAlumnos { get; set; }

    public virtual DbSet<VwSolicitudesPendiente> VwSolicitudesPendientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.HasKey(e => e.IdAlumno).HasName("alumno_pkey");

            entity.ToTable("alumno");

            entity.HasIndex(e => e.Carnet, "alumno_carnet_key").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "alumno_id_usuario_key").IsUnique();

            entity.HasIndex(e => new { e.IdSituacion, e.AnioEgresoGraduacion }, "ix_alumno_situacion");

            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.AnioEgresoGraduacion).HasColumnName("anio_egreso_graduacion");
            entity.Property(e => e.Carnet)
                .HasMaxLength(20)
                .HasColumnName("carnet");
            entity.Property(e => e.IdAreaEsp).HasColumnName("id_area_esp");
            entity.Property(e => e.IdSituacion).HasColumnName("id_situacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdAreaEspNavigation).WithMany(p => p.Alumnos)
                .HasForeignKey(d => d.IdAreaEsp)
                .HasConstraintName("alumno_id_area_esp_fkey");

            entity.HasOne(d => d.IdSituacionNavigation).WithMany(p => p.Alumnos)
                .HasForeignKey(d => d.IdSituacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alumno_id_situacion_fkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Alumno)
                .HasForeignKey<Alumno>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alumno_id_usuario_fkey");

            entity.HasMany(d => d.IdHabilidads).WithMany(p => p.IdAlumnos)
                .UsingEntity<Dictionary<string, object>>(
                    "AlumnoHabilidad",
                    r => r.HasOne<Habilidad>().WithMany()
                        .HasForeignKey("IdHabilidad")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("alumno_habilidad_id_habilidad_fkey"),
                    l => l.HasOne<Alumno>().WithMany()
                        .HasForeignKey("IdAlumno")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("alumno_habilidad_id_alumno_fkey"),
                    j =>
                    {
                        j.HasKey("IdAlumno", "IdHabilidad").HasName("alumno_habilidad_pkey");
                        j.ToTable("alumno_habilidad");
                        j.HasIndex(new[] { "IdHabilidad" }, "ix_alumno_hab_habilidad");
                        j.IndexerProperty<int>("IdAlumno").HasColumnName("id_alumno");
                        j.IndexerProperty<int>("IdHabilidad").HasColumnName("id_habilidad");
                    });
        });

        modelBuilder.Entity<AreaEspecializacion>(entity =>
        {
            entity.HasKey(e => e.IdAreaEsp).HasName("area_especializacion_pkey");

            entity.ToTable("area_especializacion");

            entity.HasIndex(e => e.Nombre, "area_especializacion_nombre_key").IsUnique();

            entity.Property(e => e.IdAreaEsp).HasColumnName("id_area_esp");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.HasKey(e => e.IdAviso).HasName("aviso_pkey");

            entity.ToTable("aviso");

            entity.HasIndex(e => e.IdAlumno, "ix_aviso_alumno");

            entity.HasIndex(e => new { e.IdOferta, e.IdAlumno }, "ux_aviso_oferta_alumno").IsUnique();

            entity.Property(e => e.IdAviso).HasColumnName("id_aviso");
            entity.Property(e => e.EstadoEnvio)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pendiente'::character varying")
                .HasColumnName("estado_envio");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.Avisos)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("aviso_id_alumno_fkey");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Avisos)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("aviso_id_oferta_fkey");
        });

        modelBuilder.Entity<ConsentimientoVisibilidad>(entity =>
        {
            entity.HasKey(e => e.IdConsentimiento).HasName("consentimiento_visibilidad_pkey");

            entity.ToTable("consentimiento_visibilidad");

            entity.HasIndex(e => e.IdAlumno, "ux_consentimiento_vigente")
                .IsUnique()
                .HasFilter("(fecha_revocado IS NULL)");

            entity.Property(e => e.IdConsentimiento).HasColumnName("id_consentimiento");
            entity.Property(e => e.FechaOtorgado)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_otorgado");
            entity.Property(e => e.FechaRevocado)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_revocado");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.ConsentimientoVisibilidads)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("consentimiento_visibilidad_id_alumno_fkey");
        });

        modelBuilder.Entity<DatosRegistro>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("datos_registro_pkey");

            entity.ToTable("datos_registro");

            entity.Property(e => e.IdUsuario)
                .ValueGeneratedNever()
                .HasColumnName("id_usuario");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FotoUrl)
                .HasMaxLength(255)
                .HasColumnName("foto_url");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.DatosRegistro)
                .HasForeignKey<DatosRegistro>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("datos_registro_id_usuario_fkey");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("empresa_pkey");

            entity.ToTable("empresa");

            entity.HasIndex(e => e.IdUsuario, "empresa_id_usuario_key").IsUnique();

            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nit)
                .HasMaxLength(20)
                .HasColumnName("nit");
            entity.Property(e => e.NombreEmpresa)
                .HasMaxLength(150)
                .HasColumnName("nombre_empresa");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(200)
                .HasColumnName("sitio_web");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Empresa)
                .HasForeignKey<Empresa>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empresa_id_usuario_fkey");
        });

        modelBuilder.Entity<EstadoCuentum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCuenta).HasName("estado_cuenta_pkey");

            entity.ToTable("estado_cuenta");

            entity.HasIndex(e => e.Nombre, "estado_cuenta_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoCuenta).HasColumnName("id_estado_cuenta");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EstadoOfertum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoOferta).HasName("estado_oferta_pkey");

            entity.ToTable("estado_oferta");

            entity.HasIndex(e => e.Nombre, "estado_oferta_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoOferta).HasColumnName("id_estado_oferta");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EstadoPostulacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPostulacion).HasName("estado_postulacion_pkey");

            entity.ToTable("estado_postulacion");

            entity.HasIndex(e => e.Nombre, "estado_postulacion_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoPostulacion).HasColumnName("id_estado_postulacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EstadoSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdEstadoSolicitud).HasName("estado_solicitud_pkey");

            entity.ToTable("estado_solicitud");

            entity.HasIndex(e => e.Nombre, "estado_solicitud_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<ExperienciaLaboral>(entity =>
        {
            entity.HasKey(e => e.IdExperiencia).HasName("experiencia_laboral_pkey");

            entity.ToTable("experiencia_laboral");

            entity.HasIndex(e => e.IdAlumno, "ix_experiencia_alumno");

            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");
            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .HasColumnName("empresa");
            entity.Property(e => e.EsActual).HasColumnName("es_actual");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Responsabilidades).HasColumnName("responsabilidades");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.ExperienciaLaborals)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("experiencia_laboral_id_alumno_fkey");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.ExperienciaLaborals)
                .HasForeignKey(d => d.IdSector)
                .HasConstraintName("experiencia_laboral_id_sector_fkey");
        });

        modelBuilder.Entity<Habilidad>(entity =>
        {
            entity.HasKey(e => e.IdHabilidad).HasName("habilidad_pkey");

            entity.ToTable("habilidad");

            entity.HasIndex(e => e.Nombre, "habilidad_nombre_key").IsUnique();

            entity.Property(e => e.IdHabilidad).HasColumnName("id_habilidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<OfertaLaboral>(entity =>
        {
            entity.HasKey(e => e.IdOferta).HasName("oferta_laboral_pkey");

            entity.ToTable("oferta_laboral");

            entity.HasIndex(e => new { e.IdEstadoOferta, e.FechaVencimiento }, "ix_oferta_estado_venc");

            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.Competencias).HasColumnName("competencias");
            entity.Property(e => e.CorreoContacto)
                .HasMaxLength(150)
                .HasColumnName("correo_contacto");
            entity.Property(e => e.Departamento)
                .HasMaxLength(60)
                .HasColumnName("departamento");
            entity.Property(e => e.DetalleUbicacion)
                .HasMaxLength(200)
                .HasColumnName("detalle_ubicacion");
            entity.Property(e => e.EmpresaExterna)
                .HasMaxLength(150)
                .HasColumnName("empresa_externa");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaPublicacion).HasColumnName("fecha_publicacion");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Funciones).HasColumnName("funciones");
            entity.Property(e => e.Horarios).HasColumnName("horarios");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdEstadoOferta).HasColumnName("id_estado_oferta");
            entity.Property(e => e.IdPublicador).HasColumnName("id_publicador");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Indicaciones).HasColumnName("indicaciones");
            entity.Property(e => e.Mision).HasColumnName("mision");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(20)
                .HasColumnName("modalidad");
            entity.Property(e => e.NivelExperiencia)
                .HasMaxLength(50)
                .HasColumnName("nivel_experiencia");
            entity.Property(e => e.Ofrecemos).HasColumnName("ofrecemos");
            entity.Property(e => e.PersonaContacto)
                .HasMaxLength(150)
                .HasColumnName("persona_contacto");
            entity.Property(e => e.PuestosCubiertos)
                .HasDefaultValue((short)0)
                .HasColumnName("puestos_cubiertos");
            entity.Property(e => e.PuestosVacantes).HasColumnName("puestos_vacantes");
            entity.Property(e => e.Requisitos).HasColumnName("requisitos");
            entity.Property(e => e.SalarioMax)
                .HasPrecision(10, 2)
                .HasColumnName("salario_max");
            entity.Property(e => e.SalarioMin)
                .HasPrecision(10, 2)
                .HasColumnName("salario_min");
            entity.Property(e => e.TipoEmpleo)
                .HasMaxLength(20)
                .HasColumnName("tipo_empleo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
            entity.Property(e => e.Vehiculo)
                .HasMaxLength(20)
                .HasColumnName("vehiculo");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.OfertaLaborals)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("oferta_laboral_id_empresa_fkey");

            entity.HasOne(d => d.IdEstadoOfertaNavigation).WithMany(p => p.OfertaLaborals)
                .HasForeignKey(d => d.IdEstadoOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("oferta_laboral_id_estado_oferta_fkey");

            entity.HasOne(d => d.IdPublicadorNavigation).WithMany(p => p.OfertaLaborals)
                .HasForeignKey(d => d.IdPublicador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("oferta_laboral_id_publicador_fkey");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.OfertaLaborals)
                .HasForeignKey(d => d.IdSector)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("oferta_laboral_id_sector_fkey");

            entity.HasMany(d => d.IdHabilidads).WithMany(p => p.IdOferta)
                .UsingEntity<Dictionary<string, object>>(
                    "OfertaHabilidad",
                    r => r.HasOne<Habilidad>().WithMany()
                        .HasForeignKey("IdHabilidad")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("oferta_habilidad_id_habilidad_fkey"),
                    l => l.HasOne<OfertaLaboral>().WithMany()
                        .HasForeignKey("IdOferta")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("oferta_habilidad_id_oferta_fkey"),
                    j =>
                    {
                        j.HasKey("IdOferta", "IdHabilidad").HasName("oferta_habilidad_pkey");
                        j.ToTable("oferta_habilidad");
                        j.HasIndex(new[] { "IdHabilidad" }, "ix_oferta_hab_habilidad");
                        j.IndexerProperty<int>("IdOferta").HasColumnName("id_oferta");
                        j.IndexerProperty<int>("IdHabilidad").HasColumnName("id_habilidad");
                    });
        });

        modelBuilder.Entity<Postulacion>(entity =>
        {
            entity.HasKey(e => e.IdPostulacion).HasName("postulacion_pkey");

            entity.ToTable("postulacion");

            entity.HasIndex(e => e.IdAlumno, "ix_postulacion_alumno");

            entity.HasIndex(e => new { e.IdOferta, e.IdAlumno }, "ux_postulacion_oferta_alumno").IsUnique();

            entity.Property(e => e.IdPostulacion).HasColumnName("id_postulacion");
            entity.Property(e => e.FechaPostulacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_postulacion");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdEstadoPostulacion).HasColumnName("id_estado_postulacion");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.Postulacions)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("postulacion_id_alumno_fkey");

            entity.HasOne(d => d.IdEstadoPostulacionNavigation).WithMany(p => p.Postulacions)
                .HasForeignKey(d => d.IdEstadoPostulacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("postulacion_id_estado_postulacion_fkey");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Postulacions)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("postulacion_id_oferta_fkey");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("rol_pkey");

            entity.ToTable("rol");

            entity.HasIndex(e => e.Nombre, "rol_nombre_key").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.HasKey(e => e.IdSector).HasName("sector_pkey");

            entity.ToTable("sector");

            entity.HasIndex(e => e.Nombre, "sector_nombre_key").IsUnique();

            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SituacionAcademica>(entity =>
        {
            entity.HasKey(e => e.IdSituacion).HasName("situacion_academica_pkey");

            entity.ToTable("situacion_academica");

            entity.HasIndex(e => e.Nombre, "situacion_academica_nombre_key").IsUnique();

            entity.Property(e => e.IdSituacion).HasColumnName("id_situacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SolicitudCuentum>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("solicitud_cuenta_pkey");

            entity.ToTable("solicitud_cuenta");

            entity.HasIndex(e => e.IdUsuario, "solicitud_cuenta_id_usuario_key").IsUnique();

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.DeclaracionVeracidad).HasColumnName("declaracion_veracidad");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");
            entity.Property(e => e.IdRevisor).HasColumnName("id_revisor");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdEstadoSolicitudNavigation).WithMany(p => p.SolicitudCuenta)
                .HasForeignKey(d => d.IdEstadoSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitud_cuenta_id_estado_solicitud_fkey");

            entity.HasOne(d => d.IdRevisorNavigation).WithMany(p => p.SolicitudCuentumIdRevisorNavigations)
                .HasForeignKey(d => d.IdRevisor)
                .HasConstraintName("solicitud_cuenta_id_revisor_fkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.SolicitudCuentumIdUsuarioNavigation)
                .HasForeignKey<SolicitudCuentum>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitud_cuenta_id_usuario_fkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuario_pkey");

            entity.ToTable("usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ContraseniaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasenia_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.IdEstadoCuenta).HasColumnName("id_estado_cuenta");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");

            entity.HasOne(d => d.IdEstadoCuentaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEstadoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuario_id_estado_cuenta_fkey");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuario_id_rol_fkey");
        });

        modelBuilder.Entity<VwCandidatosOfertum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_candidatos_oferta");

            entity.Property(e => e.Afinidad).HasColumnName("afinidad");
            entity.Property(e => e.AniosExperiencia).HasColumnName("anios_experiencia");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.EstadoPostulacion)
                .HasMaxLength(30)
                .HasColumnName("estado_postulacion");
            entity.Property(e => e.FechaPostulacion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_postulacion");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.IdPostulacion).HasColumnName("id_postulacion");
            entity.Property(e => e.IdPublicador).HasColumnName("id_publicador");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.SituacionAcademica)
                .HasMaxLength(30)
                .HasColumnName("situacion_academica");
        });

        modelBuilder.Entity<VwEmpleabilidadPorAnio>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_empleabilidad_por_anio");

            entity.Property(e => e.AlumnosConEmpleo).HasColumnName("alumnos_con_empleo");
            entity.Property(e => e.AnioEgresoGraduacion).HasColumnName("anio_egreso_graduacion");
            entity.Property(e => e.PorcentajeEmpleabilidad).HasColumnName("porcentaje_empleabilidad");
            entity.Property(e => e.SituacionAcademica)
                .HasMaxLength(30)
                .HasColumnName("situacion_academica");
            entity.Property(e => e.TotalAlumnos).HasColumnName("total_alumnos");
        });

        modelBuilder.Entity<VwMisPostulacione>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_mis_postulaciones");

            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .HasColumnName("empresa");
            entity.Property(e => e.EstadoOferta)
                .HasMaxLength(30)
                .HasColumnName("estado_oferta");
            entity.Property(e => e.EstadoPostulacion)
                .HasMaxLength(30)
                .HasColumnName("estado_postulacion");
            entity.Property(e => e.FechaPostulacion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_postulacion");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.IdPostulacion).HasColumnName("id_postulacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<VwOfertasVigente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ofertas_vigentes");

            entity.Property(e => e.Departamento)
                .HasMaxLength(60)
                .HasColumnName("departamento");
            entity.Property(e => e.DetalleUbicacion)
                .HasMaxLength(200)
                .HasColumnName("detalle_ubicacion");
            entity.Property(e => e.DiasRestantes).HasColumnName("dias_restantes");
            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .HasColumnName("empresa");
            entity.Property(e => e.FechaPublicacion).HasColumnName("fecha_publicacion");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Habilidades)
                .HasColumnType("character varying(100)[]")
                .HasColumnName("habilidades");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(20)
                .HasColumnName("modalidad");
            entity.Property(e => e.SalarioMax)
                .HasPrecision(10, 2)
                .HasColumnName("salario_max");
            entity.Property(e => e.SalarioMin)
                .HasPrecision(10, 2)
                .HasColumnName("salario_min");
            entity.Property(e => e.Sector)
                .HasMaxLength(100)
                .HasColumnName("sector");
            entity.Property(e => e.TipoEmpleo)
                .HasMaxLength(20)
                .HasColumnName("tipo_empleo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
            entity.Property(e => e.VacantesDisponibles).HasColumnName("vacantes_disponibles");
        });

        modelBuilder.Entity<VwPerfilAlumnoPublico>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_perfil_alumno_publico");

            entity.Property(e => e.AnioEgresoGraduacion).HasColumnName("anio_egreso_graduacion");
            entity.Property(e => e.AniosExperiencia).HasColumnName("anios_experiencia");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.AreaEspecializacion)
                .HasMaxLength(100)
                .HasColumnName("area_especializacion");
            entity.Property(e => e.CondicionLaboral).HasColumnName("condicion_laboral");
            entity.Property(e => e.FotoUrl)
                .HasMaxLength(255)
                .HasColumnName("foto_url");
            entity.Property(e => e.Habilidades)
                .HasColumnType("character varying(100)[]")
                .HasColumnName("habilidades");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.SituacionAcademica)
                .HasMaxLength(30)
                .HasColumnName("situacion_academica");
        });

        modelBuilder.Entity<VwResumenOferta>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_resumen_ofertas");

            entity.Property(e => e.Descartadas).HasColumnName("descartadas");
            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .HasColumnName("empresa");
            entity.Property(e => e.EnRevision).HasColumnName("en_revision");
            entity.Property(e => e.EstadoOferta)
                .HasMaxLength(30)
                .HasColumnName("estado_oferta");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdOferta).HasColumnName("id_oferta");
            entity.Property(e => e.IdPublicador).HasColumnName("id_publicador");
            entity.Property(e => e.PuestosCubiertos).HasColumnName("puestos_cubiertos");
            entity.Property(e => e.PuestosVacantes).HasColumnName("puestos_vacantes");
            entity.Property(e => e.Recibidas).HasColumnName("recibidas");
            entity.Property(e => e.Retiradas).HasColumnName("retiradas");
            entity.Property(e => e.Seleccionadas).HasColumnName("seleccionadas");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
            entity.Property(e => e.VacantesDisponibles).HasColumnName("vacantes_disponibles");
        });

        modelBuilder.Entity<VwSeguimientoAlumno>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_seguimiento_alumnos");

            entity.Property(e => e.AnioEgresoGraduacion).HasColumnName("anio_egreso_graduacion");
            entity.Property(e => e.AniosExperiencia).HasColumnName("anios_experiencia");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.AreaEspecializacion)
                .HasMaxLength(100)
                .HasColumnName("area_especializacion");
            entity.Property(e => e.CargoActual)
                .HasMaxLength(100)
                .HasColumnName("cargo_actual");
            entity.Property(e => e.Carnet)
                .HasMaxLength(20)
                .HasColumnName("carnet");
            entity.Property(e => e.CondicionLaboral).HasColumnName("condicion_laboral");
            entity.Property(e => e.ConsentimientoVigente).HasColumnName("consentimiento_vigente");
            entity.Property(e => e.EmpresaActual)
                .HasMaxLength(150)
                .HasColumnName("empresa_actual");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.SituacionAcademica)
                .HasMaxLength(30)
                .HasColumnName("situacion_academica");
        });

        modelBuilder.Entity<VwSolicitudesPendiente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_solicitudes_pendientes");

            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");
            entity.Property(e => e.Carnet)
                .HasMaxLength(20)
                .HasColumnName("carnet");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.DeclaracionVeracidad).HasColumnName("declaracion_veracidad");
            entity.Property(e => e.DiasEspera).HasColumnName("dias_espera");
            entity.Property(e => e.FechaSolicitud)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nit)
                .HasMaxLength(20)
                .HasColumnName("nit");
            entity.Property(e => e.NombreEmpresa)
                .HasMaxLength(150)
                .HasColumnName("nombre_empresa");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.Rol)
                .HasMaxLength(30)
                .HasColumnName("rol");
            entity.Property(e => e.SituacionAcademica)
                .HasMaxLength(30)
                .HasColumnName("situacion_academica");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
