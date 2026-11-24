using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alertum> Alerta { get; set; }

    public virtual DbSet<Asignacioncaso> Asignacioncasos { get; set; }

    public virtual DbSet<Auditoriaacceso> Auditoriaaccesos { get; set; }

    public virtual DbSet<Caso> Casos { get; set; }

    public virtual DbSet<Consentimientousuario> Consentimientousuarios { get; set; }

    public virtual DbSet<Conversacionchatbot> Conversacionchatbots { get; set; }

    public virtual DbSet<Conversacionprofesional> Conversacionprofesionals { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Estadoactivo> Estadoactivos { get; set; }

    public virtual DbSet<Estadoalertum> Estadoalerta { get; set; }

    public virtual DbSet<Estadoasignacion> Estadoasignacions { get; set; }

    public virtual DbSet<Estadocaso> Estadocasos { get; set; }

    public virtual DbSet<Estadosolicitud> Estadosolicituds { get; set; }

    public virtual DbSet<Factorriesgo> Factorriesgos { get; set; }

    public virtual DbSet<Mensajechatbot> Mensajechatbots { get; set; }

    public virtual DbSet<Mensajeempleadoprofesional> Mensajeempleadoprofesionals { get; set; }

    public virtual DbSet<Mensajeusuarioprofesional> Mensajeusuarioprofesionals { get; set; }

    public virtual DbSet<Nivelriesgo> Nivelriesgos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Prefijotelefonico> Prefijotelefonicos { get; set; }

    public virtual DbSet<Recursoayudum> Recursoayuda { get; set; }

    public virtual DbSet<Recursocontacto> Recursocontactos { get; set; }

    public virtual DbSet<Resultadoanalisi> Resultadoanalises { get; set; }

    public virtual DbSet<Revisionalertum> Revisionalerta { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Seguimiento> Seguimientos { get; set; }

    public virtual DbSet<Solicitudacompanamiento> Solicitudacompanamientos { get; set; }

    public virtual DbSet<Telefono> Telefonos { get; set; }

    public virtual DbSet<Tipocontactorecurso> Tipocontactorecursos { get; set; }

    public virtual DbSet<Tipoemisorchatbot> Tipoemisorchatbots { get; set; }

    public virtual DbSet<Tiporecurso> Tiporecursos { get; set; }

    public virtual DbSet<Tiporecursoauditable> Tiporecursoauditables { get; set; }

    public virtual DbSet<Tipotelefono> Tipotelefonos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alertum>(entity =>
        {
            entity.HasKey(e => e.IdAlerta);

            entity.ToTable("alerta");

            entity.HasIndex(e => e.IdAnalisis, "IX_alerta_id_analisis");

            entity.HasIndex(e => e.IdEstadoAlerta, "IX_alerta_id_estado_alerta");
            entity.Property(e => e.IdAlerta)
                .HasColumnName("id_alerta")
                .HasDefaultValueSql("(NEXT VALUE FOR dbo.Seq_Alerta)");
            entity.Property(e => e.FechaGeneracion)
                .HasPrecision(0)
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.IdAnalisis).HasColumnName("id_analisis");
            entity.Property(e => e.IdEstadoAlerta).HasColumnName("id_estado_alerta");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");

            entity.HasOne(d => d.IdAnalisisNavigation).WithMany(p => p.Alerta)
                .HasForeignKey(d => d.IdAnalisis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alerta_ibfk_1");

            entity.HasOne(d => d.IdEstadoAlertaNavigation).WithMany(p => p.Alerta)
                .HasForeignKey(d => d.IdEstadoAlerta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alerta_ibfk_2");
        });

        modelBuilder.Entity<Asignacioncaso>(entity =>
        {
            entity.HasKey(e => e.IdAsignacion);

            entity.ToTable("asignacioncaso");

            entity.HasIndex(e => e.CedulaEmpleado, "IX_asignacioncaso_cedula_empleado");

            entity.HasIndex(e => e.IdCaso, "IX_asignacioncaso_id_caso");

            entity.HasIndex(e => e.IdEstadoAsignacion, "IX_asignacioncaso_id_estado_asignacion");

            entity.Property(e => e.IdAsignacion)
                .ValueGeneratedNever()
                .HasColumnName("id_asignacion");
            entity.Property(e => e.CedulaEmpleado).HasColumnName("cedula_empleado");
            entity.Property(e => e.FechaAsignacion)
                .HasPrecision(0)
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.IdCaso).HasColumnName("id_caso");
            entity.Property(e => e.IdEstadoAsignacion).HasColumnName("id_estado_asignacion");

            entity.HasOne(d => d.CedulaEmpleadoNavigation).WithMany(p => p.Asignacioncasos)
                .HasForeignKey(d => d.CedulaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asignacioncaso_ibfk_2");

            entity.HasOne(d => d.IdCasoNavigation).WithMany(p => p.Asignacioncasos)
                .HasForeignKey(d => d.IdCaso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asignacioncaso_ibfk_1");

            entity.HasOne(d => d.IdEstadoAsignacionNavigation).WithMany(p => p.Asignacioncasos)
                .HasForeignKey(d => d.IdEstadoAsignacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asignacioncaso_ibfk_3");
        });

        modelBuilder.Entity<Auditoriaacceso>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria);

            entity.ToTable("auditoriaacceso");

            entity.HasIndex(e => e.CedulaEmpleado, "IX_auditoriaacceso_cedula_empleado");

            entity.HasIndex(e => e.IdTipoRecursoAuditable, "IX_auditoriaacceso_id_tipo_recurso_auditable");

            entity.Property(e => e.IdAuditoria)
                .ValueGeneratedNever()
                .HasColumnName("id_auditoria");
            entity.Property(e => e.CedulaEmpleado).HasColumnName("cedula_empleado");
            entity.Property(e => e.FechaAcceso)
                .HasPrecision(0)
                .HasColumnName("fecha_acceso");
            entity.Property(e => e.IdRecursoAccedido).HasColumnName("id_recurso_accedido");
            entity.Property(e => e.IdTipoRecursoAuditable).HasColumnName("id_tipo_recurso_auditable");

            entity.HasOne(d => d.CedulaEmpleadoNavigation).WithMany(p => p.Auditoriaaccesos)
                .HasForeignKey(d => d.CedulaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auditoriaacceso_ibfk_1");

            entity.HasOne(d => d.IdTipoRecursoAuditableNavigation).WithMany(p => p.Auditoriaaccesos)
                .HasForeignKey(d => d.IdTipoRecursoAuditable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auditoriaacceso_ibfk_2");
        });

        modelBuilder.Entity<Caso>(entity =>
        {
            entity.HasKey(e => e.IdCaso);

            entity.ToTable("caso");

            entity.HasIndex(e => e.CedulaUsuario, "IX_caso_cedula_usuario");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_caso_id_estado_activo");

            entity.HasIndex(e => e.IdEstadoCaso, "IX_caso_id_estado_caso");

            entity.Property(e => e.IdCaso)
                .ValueGeneratedNever()
                .HasColumnName("id_caso");
            entity.Property(e => e.CedulaUsuario).HasColumnName("cedula_usuario");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaApertura)
                .HasPrecision(0)
                .HasColumnName("fecha_apertura");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");
            entity.Property(e => e.IdEstadoCaso).HasColumnName("id_estado_caso");

            entity.HasOne(d => d.CedulaUsuarioNavigation).WithMany(p => p.Casos)
                .HasForeignKey(d => d.CedulaUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caso_ibfk_1");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Casos)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caso_ibfk_3");

            entity.HasOne(d => d.IdEstadoCasoNavigation).WithMany(p => p.Casos)
                .HasForeignKey(d => d.IdEstadoCaso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caso_ibfk_2");

            entity.HasMany(d => d.IdFactorRiesgos).WithMany(p => p.IdCasos)
                .UsingEntity<Dictionary<string, object>>(
                    "Casofactorriesgo",
                    r => r.HasOne<Factorriesgo>().WithMany()
                        .HasForeignKey("IdFactorRiesgo")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("casofactorriesgo_ibfk_2"),
                    l => l.HasOne<Caso>().WithMany()
                        .HasForeignKey("IdCaso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("casofactorriesgo_ibfk_1"),
                    j =>
                    {
                        j.HasKey("IdCaso", "IdFactorRiesgo");
                        j.ToTable("casofactorriesgo");
                        j.HasIndex(new[] { "IdFactorRiesgo" }, "IX_casofactorriesgo_id_factor_riesgo");
                        j.IndexerProperty<int>("IdCaso").HasColumnName("id_caso");
                        j.IndexerProperty<int>("IdFactorRiesgo").HasColumnName("id_factor_riesgo");
                    });

            entity.HasMany(d => d.IdSolicituds).WithMany(p => p.IdCasos)
                .UsingEntity<Dictionary<string, object>>(
                    "Casoorigensolicitud",
                    r => r.HasOne<Solicitudacompanamiento>().WithMany()
                        .HasForeignKey("IdSolicitud")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("casoorigensolicitud_ibfk_2"),
                    l => l.HasOne<Caso>().WithMany()
                        .HasForeignKey("IdCaso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("casoorigensolicitud_ibfk_1"),
                    j =>
                    {
                        j.HasKey("IdCaso", "IdSolicitud");
                        j.ToTable("casoorigensolicitud");
                        j.HasIndex(new[] { "IdSolicitud" }, "IX_casoorigensolicitud_id_solicitud");
                        j.IndexerProperty<int>("IdCaso").HasColumnName("id_caso");
                        j.IndexerProperty<int>("IdSolicitud").HasColumnName("id_solicitud");
                    });
        });

        modelBuilder.Entity<Consentimientousuario>(entity =>
        {
            entity.HasKey(e => e.IdConsentimiento);

            entity.ToTable("consentimientousuario");

            entity.HasIndex(e => e.CedulaUsuario, "IX_consentimientousuario_cedula_usuario");

            entity.Property(e => e.IdConsentimiento)
                .ValueGeneratedNever()
                .HasColumnName("id_consentimiento");
            entity.Property(e => e.CedulaUsuario).HasColumnName("cedula_usuario");
            entity.Property(e => e.FechaAceptacion)
                .HasPrecision(0)
                .HasColumnName("fecha_aceptacion");
            entity.Property(e => e.VersionConsentimiento)
                .HasMaxLength(20)
                .HasColumnName("version_consentimiento");

            entity.HasOne(d => d.CedulaUsuarioNavigation).WithMany(p => p.Consentimientousuarios)
                .HasForeignKey(d => d.CedulaUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("consentimientousuario_ibfk_1");
        });

        modelBuilder.Entity<Conversacionchatbot>(entity =>
        {
                    entity.Property(e => e.OcultaUsuario)
            .HasColumnName("oculta_usuario")
            .HasDefaultValue(false);
            entity.HasKey(e => e.IdConversacion);

            entity.ToTable("conversacionchatbot");

            entity.HasIndex(e => e.CedulaUsuario, "IX_conversacionchatbot_cedula_usuario");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_conversacionchatbot_id_estado_activo");

            entity.Property(e => e.IdConversacion)
    .HasColumnName("id_conversacion")
    .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[Seq_ConversacionChatbot])")
    .ValueGeneratedOnAdd();

            entity.Property(e => e.CedulaUsuario).HasColumnName("cedula_usuario");
            entity.Property(e => e.FechaInicio)
                .HasPrecision(0)
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaUltimaActividad)
                .HasPrecision(0)
                .HasColumnName("fecha_ultima_actividad");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");

            entity.HasOne(d => d.CedulaUsuarioNavigation).WithMany(p => p.Conversacionchatbots)
                .HasForeignKey(d => d.CedulaUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversacionchatbot_ibfk_1");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Conversacionchatbots)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversacionchatbot_ibfk_2");
        });

        modelBuilder.Entity<Conversacionprofesional>(entity =>
        {
            entity.HasKey(e => e.IdConversacionProfesional);

            entity.ToTable("conversacionprofesional");

            entity.HasIndex(e => e.IdCaso, "IX_conversacionprofesional_id_caso");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_conversacionprofesional_id_estado_activo");

            entity.Property(e => e.IdConversacionProfesional)
                .ValueGeneratedNever()
                .HasColumnName("id_conversacion_profesional");
            entity.Property(e => e.FechaInicio)
                .HasPrecision(0)
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IdCaso).HasColumnName("id_caso");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");

            entity.HasOne(d => d.IdCasoNavigation).WithMany(p => p.Conversacionprofesionals)
                .HasForeignKey(d => d.IdCaso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversacionprofesional_ibfk_1");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Conversacionprofesionals)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversacionprofesional_ibfk_2");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Cedula);

            entity.ToTable("empleado");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_empleado_id_estado_activo");

            entity.HasIndex(e => e.Correo, "UQ_empleado_correo").IsUnique();

            entity.Property(e => e.Cedula)
                .ValueGeneratedNever()
                .HasColumnName("cedula");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaContratacion)
                .HasPrecision(0)
                .HasColumnName("fecha_contratacion");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");

            entity.HasOne(d => d.CedulaNavigation).WithOne(p => p.Empleado)
                .HasForeignKey<Empleado>(d => d.Cedula)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleado_ibfk_1");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleado_ibfk_2");

            entity.HasMany(d => d.IdRols).WithMany(p => p.CedulaEmpleados)
                .UsingEntity<Dictionary<string, object>>(
                    "Empleadorol",
                    r => r.HasOne<Rol>().WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("empleadorol_ibfk_2"),
                    l => l.HasOne<Empleado>().WithMany()
                        .HasForeignKey("CedulaEmpleado")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("empleadorol_ibfk_1"),
                    j =>
                    {
                        j.HasKey("CedulaEmpleado", "IdRol");
                        j.ToTable("empleadorol");
                        j.HasIndex(new[] { "IdRol" }, "IX_empleadorol_id_rol");
                        j.IndexerProperty<int>("CedulaEmpleado").HasColumnName("cedula_empleado");
                        j.IndexerProperty<int>("IdRol").HasColumnName("id_rol");
                    });

            entity.HasMany(d => d.IdTelefonos).WithMany(p => p.CedulaEmpleados)
                .UsingEntity<Dictionary<string, object>>(
                    "Empleadotelefono",
                    r => r.HasOne<Telefono>().WithMany()
                        .HasForeignKey("IdTelefono")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("empleadotelefono_ibfk_2"),
                    l => l.HasOne<Empleado>().WithMany()
                        .HasForeignKey("CedulaEmpleado")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("empleadotelefono_ibfk_1"),
                    j =>
                    {
                        j.HasKey("CedulaEmpleado", "IdTelefono");
                        j.ToTable("empleadotelefono");
                        j.HasIndex(new[] { "IdTelefono" }, "IX_empleadotelefono_id_telefono");
                        j.IndexerProperty<int>("CedulaEmpleado").HasColumnName("cedula_empleado");
                        j.IndexerProperty<int>("IdTelefono").HasColumnName("id_telefono");
                    });
        });

        modelBuilder.Entity<Estadoactivo>(entity =>
        {
            entity.HasKey(e => e.IdEstadoActivo);

            entity.ToTable("estadoactivo");

            entity.HasIndex(e => e.Nombre, "UQ_estadoactivo_nombre").IsUnique();

            entity.Property(e => e.IdEstadoActivo)
                .ValueGeneratedNever()
                .HasColumnName("id_estado_activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estadoalertum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoAlerta);

            entity.ToTable("estadoalerta");

            entity.HasIndex(e => e.Nombre, "UQ_estadoalerta_nombre").IsUnique();

            entity.Property(e => e.IdEstadoAlerta)
                .ValueGeneratedNever()
                .HasColumnName("id_estado_alerta");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estadoasignacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoAsignacion);

            entity.ToTable("estadoasignacion");

            entity.HasIndex(e => e.Nombre, "UQ_estadoasignacion_nombre").IsUnique();

            entity.Property(e => e.IdEstadoAsignacion)
                .ValueGeneratedNever()
                .HasColumnName("id_estado_asignacion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estadocaso>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCaso);

            entity.ToTable("estadocaso");

            entity.HasIndex(e => e.Nombre, "UQ_estadocaso_nombre").IsUnique();

            entity.Property(e => e.IdEstadoCaso)
                .ValueGeneratedNever()
                .HasColumnName("id_estado_caso");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estadosolicitud>(entity =>
        {
            entity.HasKey(e => e.IdEstadoSolicitud);

            entity.ToTable("estadosolicitud");

            entity.HasIndex(e => e.Nombre, "UQ_estadosolicitud_nombre").IsUnique();

            entity.Property(e => e.IdEstadoSolicitud)
                .ValueGeneratedNever()
                .HasColumnName("id_estado_solicitud");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Factorriesgo>(entity =>
        {
            entity.HasKey(e => e.IdFactorRiesgo);

            entity.ToTable("factorriesgo");

            entity.HasIndex(e => e.Nombre, "UQ_factorriesgo_nombre").IsUnique();

            entity.Property(e => e.IdFactorRiesgo)
                .ValueGeneratedNever()
                .HasColumnName("id_factor_riesgo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Mensajechatbot>(entity =>
        {
            entity.HasKey(e => e.IdMensaje);

            entity.ToTable("mensajechatbot");

            entity.HasIndex(e => e.IdConversacion, "IX_mensajechatbot_id_conversacion");

            entity.HasIndex(e => e.IdTipoEmisor, "IX_mensajechatbot_id_tipo_emisor");

            entity.Property(e => e.IdMensaje)
     .HasColumnName("id_mensaje")
     .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[Seq_MensajeChatbot])")
     .ValueGeneratedOnAdd();

            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.FechaEnvio)
                .HasPrecision(0)
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdConversacion).HasColumnName("id_conversacion");
            entity.Property(e => e.IdTipoEmisor).HasColumnName("id_tipo_emisor");

            entity.HasOne(d => d.IdConversacionNavigation).WithMany(p => p.Mensajechatbots)
                .HasForeignKey(d => d.IdConversacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajechatbot_ibfk_1");

            entity.HasOne(d => d.IdTipoEmisorNavigation).WithMany(p => p.Mensajechatbots)
                .HasForeignKey(d => d.IdTipoEmisor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajechatbot_ibfk_2");
        });

        modelBuilder.Entity<Mensajeempleadoprofesional>(entity =>
        {
            entity.HasKey(e => e.IdMensajeEmpleado);

            entity.ToTable("mensajeempleadoprofesional");

            entity.HasIndex(e => e.CedulaEmpleado, "IX_mensajeempleadoprofesional_cedula_empleado");

            entity.HasIndex(e => e.IdConversacionProfesional, "IX_mensajeempleadoprofesional_id_conversacion_profesional");

            entity.Property(e => e.IdMensajeEmpleado)
                .ValueGeneratedNever()
                .HasColumnName("id_mensaje_empleado");
            entity.Property(e => e.CedulaEmpleado).HasColumnName("cedula_empleado");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.FechaEnvio)
                .HasPrecision(0)
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdConversacionProfesional).HasColumnName("id_conversacion_profesional");

            entity.HasOne(d => d.CedulaEmpleadoNavigation).WithMany(p => p.Mensajeempleadoprofesionals)
                .HasForeignKey(d => d.CedulaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajeempleadoprofesional_ibfk_2");

            entity.HasOne(d => d.IdConversacionProfesionalNavigation).WithMany(p => p.Mensajeempleadoprofesionals)
                .HasForeignKey(d => d.IdConversacionProfesional)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajeempleadoprofesional_ibfk_1");
        });

        modelBuilder.Entity<Mensajeusuarioprofesional>(entity =>
        {
            entity.HasKey(e => e.IdMensajeUsuario);

            entity.ToTable("mensajeusuarioprofesional");

            entity.HasIndex(e => e.CedulaUsuario, "IX_mensajeusuarioprofesional_cedula_usuario");

            entity.HasIndex(e => e.IdConversacionProfesional, "IX_mensajeusuarioprofesional_id_conversacion_profesional");

            entity.Property(e => e.IdMensajeUsuario)
                .ValueGeneratedNever()
                .HasColumnName("id_mensaje_usuario");
            entity.Property(e => e.CedulaUsuario).HasColumnName("cedula_usuario");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.FechaEnvio)
                .HasPrecision(0)
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdConversacionProfesional).HasColumnName("id_conversacion_profesional");

            entity.HasOne(d => d.CedulaUsuarioNavigation).WithMany(p => p.Mensajeusuarioprofesionals)
                .HasForeignKey(d => d.CedulaUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajeusuarioprofesional_ibfk_2");

            entity.HasOne(d => d.IdConversacionProfesionalNavigation).WithMany(p => p.Mensajeusuarioprofesionals)
                .HasForeignKey(d => d.IdConversacionProfesional)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mensajeusuarioprofesional_ibfk_1");
        });

        modelBuilder.Entity<Nivelriesgo>(entity =>
        {
            entity.HasKey(e => e.IdNivelRiesgo);

            entity.ToTable("nivelriesgo");

            entity.HasIndex(e => e.Nombre, "UQ_nivelriesgo_nombre").IsUnique();

            entity.Property(e => e.IdNivelRiesgo)
                .ValueGeneratedNever()
                .HasColumnName("id_nivel_riesgo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.Cedula);

            entity.ToTable("persona");

            entity.Property(e => e.Cedula)
                .ValueGeneratedNever()
                .HasColumnName("cedula");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(80)
                .HasColumnName("primer_apellido");
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(80)
                .HasColumnName("segundo_apellido");
        });

        modelBuilder.Entity<Prefijotelefonico>(entity =>
        {
            entity.HasKey(e => e.IdPrefijo);

            entity.ToTable("prefijotelefonico");

            entity.HasIndex(e => e.Pais, "UQ_prefijotelefonico_pais").IsUnique();

            entity.HasIndex(e => e.Prefijo, "UQ_prefijotelefonico_prefijo").IsUnique();

            entity.Property(e => e.IdPrefijo)
                .ValueGeneratedNever()
                .HasColumnName("id_prefijo");
            entity.Property(e => e.Pais)
                .HasMaxLength(80)
                .HasColumnName("pais");
            entity.Property(e => e.Prefijo).HasColumnName("prefijo");
        });

        modelBuilder.Entity<Recursoayudum>(entity =>
        {
            entity.HasKey(e => e.IdRecurso);

            entity.ToTable("recursoayuda");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_recursoayuda_id_estado_activo");

            entity.HasIndex(e => e.IdTipoRecurso, "IX_recursoayuda_id_tipo_recurso");

            entity.HasIndex(e => e.Nombre, "UQ_recursoayuda_nombre").IsUnique();

            entity.Property(e => e.IdRecurso)
                .ValueGeneratedNever()
                .HasColumnName("id_recurso");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");
            entity.Property(e => e.IdTipoRecurso).HasColumnName("id_tipo_recurso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Recursoayuda)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recursoayuda_ibfk_2");

            entity.HasOne(d => d.IdTipoRecursoNavigation).WithMany(p => p.Recursoayuda)
                .HasForeignKey(d => d.IdTipoRecurso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recursoayuda_ibfk_1");
        });

        modelBuilder.Entity<Recursocontacto>(entity =>
        {
            entity.HasKey(e => e.IdRecursoContacto);

            entity.ToTable("recursocontacto");

            entity.HasIndex(e => e.IdRecurso, "IX_recursocontacto_id_recurso");

            entity.HasIndex(e => e.IdTipoContacto, "IX_recursocontacto_id_tipo_contacto");

            entity.Property(e => e.IdRecursoContacto)
                .ValueGeneratedNever()
                .HasColumnName("id_recurso_contacto");
            entity.Property(e => e.IdRecurso).HasColumnName("id_recurso");
            entity.Property(e => e.IdTipoContacto).HasColumnName("id_tipo_contacto");
            entity.Property(e => e.Valor)
                .HasMaxLength(300)
                .HasColumnName("valor");

            entity.HasOne(d => d.IdRecursoNavigation).WithMany(p => p.Recursocontactos)
                .HasForeignKey(d => d.IdRecurso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recursocontacto_ibfk_1");

            entity.HasOne(d => d.IdTipoContactoNavigation).WithMany(p => p.Recursocontactos)
                .HasForeignKey(d => d.IdTipoContacto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recursocontacto_ibfk_2");
        });

        modelBuilder.Entity<Resultadoanalisi>(entity =>
        {
            entity.HasKey(e => e.IdAnalisis);

            entity.ToTable("resultadoanalisis");

            entity.HasIndex(e => e.IdMensaje, "IX_resultadoanalisis_id_mensaje");

            entity.HasIndex(e => e.IdNivelRiesgo, "IX_resultadoanalisis_id_nivel_riesgo");
            entity.Property(e => e.IdAnalisis)
    .HasColumnName("id_analisis")
    .HasDefaultValueSql("(NEXT VALUE FOR dbo.Seq_ResultadoAnalisis)");
            entity.Property(e => e.FechaAnalisis)
                .HasPrecision(0)
                .HasColumnName("fecha_analisis");
            entity.Property(e => e.IdMensaje).HasColumnName("id_mensaje");
            entity.Property(e => e.IdNivelRiesgo).HasColumnName("id_nivel_riesgo");
            entity.Property(e => e.Resultado).HasColumnName("resultado");

            entity.HasOne(d => d.IdMensajeNavigation).WithMany(p => p.Resultadoanalisis)
                .HasForeignKey(d => d.IdMensaje)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("resultadoanalisis_ibfk_1");

            entity.HasOne(d => d.IdNivelRiesgoNavigation).WithMany(p => p.Resultadoanalisis)
                .HasForeignKey(d => d.IdNivelRiesgo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("resultadoanalisis_ibfk_2");
        });

        modelBuilder.Entity<Revisionalertum>(entity =>
        {
            entity.HasKey(e => e.IdRevisionAlerta);

            entity.ToTable("revisionalerta");

            entity.HasIndex(e => e.CedulaEmpleado, "IX_revisionalerta_cedula_empleado");

            entity.HasIndex(e => e.IdAlerta, "IX_revisionalerta_id_alerta");

            entity.HasIndex(e => e.IdEstadoAlerta, "IX_revisionalerta_id_estado_alerta");

            entity.Property(e => e.IdRevisionAlerta)
                .ValueGeneratedNever()
                .HasColumnName("id_revision_alerta");
            entity.Property(e => e.CedulaEmpleado).HasColumnName("cedula_empleado");
            entity.Property(e => e.FechaRevision)
                .HasPrecision(0)
                .HasColumnName("fecha_revision");
            entity.Property(e => e.IdAlerta).HasColumnName("id_alerta");
            entity.Property(e => e.IdEstadoAlerta).HasColumnName("id_estado_alerta");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");

            entity.HasOne(d => d.CedulaEmpleadoNavigation).WithMany(p => p.Revisionalerta)
                .HasForeignKey(d => d.CedulaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("revisionalerta_ibfk_2");

            entity.HasOne(d => d.IdAlertaNavigation).WithMany(p => p.Revisionalerta)
                .HasForeignKey(d => d.IdAlerta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("revisionalerta_ibfk_1");

            entity.HasOne(d => d.IdEstadoAlertaNavigation).WithMany(p => p.Revisionalerta)
                .HasForeignKey(d => d.IdEstadoAlerta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("revisionalerta_ibfk_3");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.ToTable("rol");

            entity.HasIndex(e => e.Nombre, "UQ_rol_nombre").IsUnique();

            entity.Property(e => e.IdRol)
                .ValueGeneratedNever()
                .HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Seguimiento>(entity =>
        {
            entity.HasKey(e => e.IdSeguimiento);

            entity.ToTable("seguimiento");

            entity.HasIndex(e => e.CedulaEmpleado, "IX_seguimiento_cedula_empleado");

            entity.HasIndex(e => e.IdCaso, "IX_seguimiento_id_caso");

            entity.Property(e => e.IdSeguimiento)
                .ValueGeneratedNever()
                .HasColumnName("id_seguimiento");
            entity.Property(e => e.CedulaEmpleado).HasColumnName("cedula_empleado");
            entity.Property(e => e.FechaSeguimiento)
                .HasPrecision(0)
                .HasColumnName("fecha_seguimiento");
            entity.Property(e => e.IdCaso).HasColumnName("id_caso");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");

            entity.HasOne(d => d.CedulaEmpleadoNavigation).WithMany(p => p.Seguimientos)
                .HasForeignKey(d => d.CedulaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seguimiento_ibfk_2");

            entity.HasOne(d => d.IdCasoNavigation).WithMany(p => p.Seguimientos)
                .HasForeignKey(d => d.IdCaso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seguimiento_ibfk_1");
        });

        modelBuilder.Entity<Solicitudacompanamiento>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud);

            entity.ToTable("solicitudacompanamiento");

            entity.HasIndex(e => e.CedulaUsuario, "IX_solicitudacompanamiento_cedula_usuario");

            entity.HasIndex(e => e.IdEstadoSolicitud, "IX_solicitudacompanamiento_id_estado_solicitud");

            entity.Property(e => e.IdSolicitud)
                .ValueGeneratedNever()
                .HasColumnName("id_solicitud");
            entity.Property(e => e.CedulaUsuario).HasColumnName("cedula_usuario");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FechaSolicitud)
                .HasPrecision(0)
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");

            entity.HasOne(d => d.CedulaUsuarioNavigation).WithMany(p => p.Solicitudacompanamientos)
                .HasForeignKey(d => d.CedulaUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitudacompanamiento_ibfk_1");

            entity.HasOne(d => d.IdEstadoSolicitudNavigation).WithMany(p => p.Solicitudacompanamientos)
                .HasForeignKey(d => d.IdEstadoSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitudacompanamiento_ibfk_2");
        });

        modelBuilder.Entity<Telefono>(entity =>
        {
            entity.HasKey(e => e.IdTelefono);

            entity.ToTable("telefono");

            entity.HasIndex(e => e.IdPrefijo, "IX_telefono_id_prefijo");

            entity.HasIndex(e => e.IdTipoTelefono, "IX_telefono_id_tipo_telefono");

            entity.Property(e => e.IdTelefono)
                .ValueGeneratedNever()
                .HasColumnName("id_telefono");
            entity.Property(e => e.IdPrefijo).HasColumnName("id_prefijo");
            entity.Property(e => e.IdTipoTelefono).HasColumnName("id_tipo_telefono");
            entity.Property(e => e.Numero).HasColumnName("numero");

            entity.HasOne(d => d.IdPrefijoNavigation).WithMany(p => p.Telefonos)
                .HasForeignKey(d => d.IdPrefijo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("telefono_ibfk_1");

            entity.HasOne(d => d.IdTipoTelefonoNavigation).WithMany(p => p.Telefonos)
                .HasForeignKey(d => d.IdTipoTelefono)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("telefono_ibfk_2");
        });

        modelBuilder.Entity<Tipocontactorecurso>(entity =>
        {
            entity.HasKey(e => e.IdTipoContacto);

            entity.ToTable("tipocontactorecurso");

            entity.HasIndex(e => e.Nombre, "UQ_tipocontactorecurso_nombre").IsUnique();

            entity.Property(e => e.IdTipoContacto)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_contacto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tipoemisorchatbot>(entity =>
        {
            entity.HasKey(e => e.IdTipoEmisor);

            entity.ToTable("tipoemisorchatbot");

            entity.HasIndex(e => e.Nombre, "UQ_tipoemisorchatbot_nombre").IsUnique();

            entity.Property(e => e.IdTipoEmisor)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_emisor");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tiporecurso>(entity =>
        {
            entity.HasKey(e => e.IdTipoRecurso);

            entity.ToTable("tiporecurso");

            entity.HasIndex(e => e.Nombre, "UQ_tiporecurso_nombre").IsUnique();

            entity.Property(e => e.IdTipoRecurso)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_recurso");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tiporecursoauditable>(entity =>
        {
            entity.HasKey(e => e.IdTipoRecursoAuditable);

            entity.ToTable("tiporecursoauditable");

            entity.HasIndex(e => e.Nombre, "UQ_tiporecursoauditable_nombre").IsUnique();

            entity.Property(e => e.IdTipoRecursoAuditable)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_recurso_auditable");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Tipotelefono>(entity =>
        {
            entity.HasKey(e => e.IdTipoTelefono);

            entity.ToTable("tipotelefono");

            entity.HasIndex(e => e.Nombre, "UQ_tipotelefono_nombre").IsUnique();

            entity.Property(e => e.IdTipoTelefono)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_telefono");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Cedula);

            entity.ToTable("usuario");

            entity.HasIndex(e => e.IdEstadoActivo, "IX_usuario_id_estado_activo");

            entity.HasIndex(e => e.Correo, "UQ_usuario_correo").IsUnique();

            entity.Property(e => e.Cedula)
                .ValueGeneratedNever()
                .HasColumnName("cedula");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstadoActivo).HasColumnName("id_estado_activo");

            entity.HasOne(d => d.CedulaNavigation).WithOne(p => p.Usuario)
                .HasForeignKey<Usuario>(d => d.Cedula)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuario_ibfk_1");

            entity.HasOne(d => d.IdEstadoActivoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuario_ibfk_2");

            entity.HasMany(d => d.IdTelefonos).WithMany(p => p.CedulaUsuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "Usuariotelefono",
                    r => r.HasOne<Telefono>().WithMany()
                        .HasForeignKey("IdTelefono")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("usuariotelefono_ibfk_2"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("CedulaUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("usuariotelefono_ibfk_1"),
                    j =>
                    {
                        j.HasKey("CedulaUsuario", "IdTelefono");
                        j.ToTable("usuariotelefono");
                        j.HasIndex(new[] { "IdTelefono" }, "IX_usuariotelefono_id_telefono");
                        j.IndexerProperty<int>("CedulaUsuario").HasColumnName("cedula_usuario");
                        j.IndexerProperty<int>("IdTelefono").HasColumnName("id_telefono");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
