/*
    Conversión de MySQL Workbench a Microsoft SQL Server (SSMS).
    Base de datos: fundacion_prevencion

    Notas:
    - Se conservan las 39 tablas, sus columnas, claves primarias,
      claves foráneas, restricciones UNIQUE e índices.
    - VARCHAR se convierte a NVARCHAR y TEXT a NVARCHAR(MAX) para
      conservar correctamente caracteres Unicode (equivalente práctico de utf8mb4).
    - DATETIME se convierte a DATETIME2(0), manteniendo fecha y hora
      con precisión de segundos.
    - Este archivo de origen contiene únicamente la estructura (DDL);
      no incluye instrucciones INSERT ni registros de datos.
    - Los identificadores no se generan automáticamente porque en el
      diseño original son INT NOT NULL sin AUTO_INCREMENT.
*/

IF DB_ID(N'fundacion_prevencion') IS NULL
BEGIN
    CREATE DATABASE [fundacion_prevencion];
END;
GO

USE [fundacion_prevencion];
GO

CREATE TABLE [dbo].[persona] (
    [cedula] INT NOT NULL,
    [nombre] NVARCHAR(80) NOT NULL,
    [primer_apellido] NVARCHAR(80) NOT NULL,
    [segundo_apellido] NVARCHAR(80) NOT NULL,
    CONSTRAINT [PK_persona] PRIMARY KEY ([cedula])
);

CREATE TABLE [dbo].[estadoactivo] (
    [id_estado_activo] INT NOT NULL,
    [nombre] NVARCHAR(30) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_estadoactivo] PRIMARY KEY ([id_estado_activo]),
    CONSTRAINT [UQ_estadoactivo_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[usuario] (
    [cedula] INT NOT NULL,
    [correo] NVARCHAR(150) NOT NULL,
    [contrasena_hash] NVARCHAR(255) NOT NULL,
    [fecha_registro] DATETIME2(0) NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_usuario] PRIMARY KEY ([cedula]),
    CONSTRAINT [UQ_usuario_correo] UNIQUE ([correo]),
    CONSTRAINT [usuario_ibfk_1] FOREIGN KEY ([cedula]) REFERENCES [dbo].[persona] ([cedula]),
    CONSTRAINT [usuario_ibfk_2] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[conversacionchatbot] (
    [id_conversacion] INT NOT NULL,
    [cedula_usuario] INT NOT NULL,
    [fecha_inicio] DATETIME2(0) NOT NULL,
    [fecha_ultima_actividad] DATETIME2(0) NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_conversacionchatbot] PRIMARY KEY ([id_conversacion]),
    CONSTRAINT [conversacionchatbot_ibfk_1] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula]),
    CONSTRAINT [conversacionchatbot_ibfk_2] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[tipoemisorchatbot] (
    [id_tipo_emisor] INT NOT NULL,
    [nombre] NVARCHAR(30) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_tipoemisorchatbot] PRIMARY KEY ([id_tipo_emisor]),
    CONSTRAINT [UQ_tipoemisorchatbot_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[mensajechatbot] (
    [id_mensaje] INT NOT NULL,
    [id_conversacion] INT NOT NULL,
    [id_tipo_emisor] INT NOT NULL,
    [contenido] NVARCHAR(MAX) NOT NULL,
    [fecha_envio] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_mensajechatbot] PRIMARY KEY ([id_mensaje]),
    CONSTRAINT [mensajechatbot_ibfk_1] FOREIGN KEY ([id_conversacion]) REFERENCES [dbo].[conversacionchatbot] ([id_conversacion]),
    CONSTRAINT [mensajechatbot_ibfk_2] FOREIGN KEY ([id_tipo_emisor]) REFERENCES [dbo].[tipoemisorchatbot] ([id_tipo_emisor])
);

CREATE TABLE [dbo].[nivelriesgo] (
    [id_nivel_riesgo] INT NOT NULL,
    [nombre] NVARCHAR(30) NOT NULL,
    [descripcion] NVARCHAR(200) NOT NULL,
    CONSTRAINT [PK_nivelriesgo] PRIMARY KEY ([id_nivel_riesgo]),
    CONSTRAINT [UQ_nivelriesgo_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[resultadoanalisis] (
    [id_analisis] INT NOT NULL,
    [id_mensaje] INT NOT NULL,
    [id_nivel_riesgo] INT NOT NULL,
    [resultado] NVARCHAR(MAX) NOT NULL,
    [fecha_analisis] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_resultadoanalisis] PRIMARY KEY ([id_analisis]),
    CONSTRAINT [resultadoanalisis_ibfk_1] FOREIGN KEY ([id_mensaje]) REFERENCES [dbo].[mensajechatbot] ([id_mensaje]),
    CONSTRAINT [resultadoanalisis_ibfk_2] FOREIGN KEY ([id_nivel_riesgo]) REFERENCES [dbo].[nivelriesgo] ([id_nivel_riesgo])
);

CREATE TABLE [dbo].[estadoalerta] (
    [id_estado_alerta] INT NOT NULL,
    [nombre] NVARCHAR(50) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_estadoalerta] PRIMARY KEY ([id_estado_alerta]),
    CONSTRAINT [UQ_estadoalerta_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[alerta] (
    [id_alerta] INT NOT NULL,
    [id_analisis] INT NOT NULL,
    [id_estado_alerta] INT NOT NULL,
    [fecha_generacion] DATETIME2(0) NOT NULL,
    [observaciones] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_alerta] PRIMARY KEY ([id_alerta]),
    CONSTRAINT [alerta_ibfk_1] FOREIGN KEY ([id_analisis]) REFERENCES [dbo].[resultadoanalisis] ([id_analisis]),
    CONSTRAINT [alerta_ibfk_2] FOREIGN KEY ([id_estado_alerta]) REFERENCES [dbo].[estadoalerta] ([id_estado_alerta])
);

CREATE TABLE [dbo].[estadocaso] (
    [id_estado_caso] INT NOT NULL,
    [nombre] NVARCHAR(40) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_estadocaso] PRIMARY KEY ([id_estado_caso]),
    CONSTRAINT [UQ_estadocaso_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[caso] (
    [id_caso] INT NOT NULL,
    [cedula_usuario] INT NOT NULL,
    [fecha_apertura] DATETIME2(0) NOT NULL,
    [descripcion] NVARCHAR(MAX) NOT NULL,
    [id_estado_caso] INT NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_caso] PRIMARY KEY ([id_caso]),
    CONSTRAINT [caso_ibfk_1] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula]),
    CONSTRAINT [caso_ibfk_2] FOREIGN KEY ([id_estado_caso]) REFERENCES [dbo].[estadocaso] ([id_estado_caso]),
    CONSTRAINT [caso_ibfk_3] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[empleado] (
    [cedula] INT NOT NULL,
    [correo] NVARCHAR(150) NOT NULL,
    [contrasena_hash] NVARCHAR(255) NOT NULL,
    [fecha_contratacion] DATETIME2(0) NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_empleado] PRIMARY KEY ([cedula]),
    CONSTRAINT [UQ_empleado_correo] UNIQUE ([correo]),
    CONSTRAINT [empleado_ibfk_1] FOREIGN KEY ([cedula]) REFERENCES [dbo].[persona] ([cedula]),
    CONSTRAINT [empleado_ibfk_2] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[estadoasignacion] (
    [id_estado_asignacion] INT NOT NULL,
    [nombre] NVARCHAR(40) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_estadoasignacion] PRIMARY KEY ([id_estado_asignacion]),
    CONSTRAINT [UQ_estadoasignacion_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[asignacioncaso] (
    [id_asignacion] INT NOT NULL,
    [id_caso] INT NOT NULL,
    [cedula_empleado] INT NOT NULL,
    [fecha_asignacion] DATETIME2(0) NOT NULL,
    [id_estado_asignacion] INT NOT NULL,
    CONSTRAINT [PK_asignacioncaso] PRIMARY KEY ([id_asignacion]),
    CONSTRAINT [asignacioncaso_ibfk_1] FOREIGN KEY ([id_caso]) REFERENCES [dbo].[caso] ([id_caso]),
    CONSTRAINT [asignacioncaso_ibfk_2] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula]),
    CONSTRAINT [asignacioncaso_ibfk_3] FOREIGN KEY ([id_estado_asignacion]) REFERENCES [dbo].[estadoasignacion] ([id_estado_asignacion])
);

CREATE TABLE [dbo].[tiporecursoauditable] (
    [id_tipo_recurso_auditable] INT NOT NULL,
    [nombre] NVARCHAR(50) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_tiporecursoauditable] PRIMARY KEY ([id_tipo_recurso_auditable]),
    CONSTRAINT [UQ_tiporecursoauditable_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[auditoriaacceso] (
    [id_auditoria] INT NOT NULL,
    [cedula_empleado] INT NOT NULL,
    [id_tipo_recurso_auditable] INT NOT NULL,
    [id_recurso_accedido] INT NOT NULL,
    [fecha_acceso] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_auditoriaacceso] PRIMARY KEY ([id_auditoria]),
    CONSTRAINT [auditoriaacceso_ibfk_1] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula]),
    CONSTRAINT [auditoriaacceso_ibfk_2] FOREIGN KEY ([id_tipo_recurso_auditable]) REFERENCES [dbo].[tiporecursoauditable] ([id_tipo_recurso_auditable])
);

CREATE TABLE [dbo].[factorriesgo] (
    [id_factor_riesgo] INT NOT NULL,
    [nombre] NVARCHAR(100) NOT NULL,
    [descripcion] NVARCHAR(300) NOT NULL,
    CONSTRAINT [PK_factorriesgo] PRIMARY KEY ([id_factor_riesgo]),
    CONSTRAINT [UQ_factorriesgo_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[casofactorriesgo] (
    [id_caso] INT NOT NULL,
    [id_factor_riesgo] INT NOT NULL,
    CONSTRAINT [PK_casofactorriesgo] PRIMARY KEY ([id_caso], [id_factor_riesgo]),
    CONSTRAINT [casofactorriesgo_ibfk_1] FOREIGN KEY ([id_caso]) REFERENCES [dbo].[caso] ([id_caso]),
    CONSTRAINT [casofactorriesgo_ibfk_2] FOREIGN KEY ([id_factor_riesgo]) REFERENCES [dbo].[factorriesgo] ([id_factor_riesgo])
);

CREATE TABLE [dbo].[estadosolicitud] (
    [id_estado_solicitud] INT NOT NULL,
    [nombre] NVARCHAR(40) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_estadosolicitud] PRIMARY KEY ([id_estado_solicitud]),
    CONSTRAINT [UQ_estadosolicitud_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[solicitudacompanamiento] (
    [id_solicitud] INT NOT NULL,
    [cedula_usuario] INT NOT NULL,
    [fecha_solicitud] DATETIME2(0) NOT NULL,
    [descripcion] NVARCHAR(MAX) NOT NULL,
    [id_estado_solicitud] INT NOT NULL,
    CONSTRAINT [PK_solicitudacompanamiento] PRIMARY KEY ([id_solicitud]),
    CONSTRAINT [solicitudacompanamiento_ibfk_1] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula]),
    CONSTRAINT [solicitudacompanamiento_ibfk_2] FOREIGN KEY ([id_estado_solicitud]) REFERENCES [dbo].[estadosolicitud] ([id_estado_solicitud])
);

CREATE TABLE [dbo].[casoorigensolicitud] (
    [id_caso] INT NOT NULL,
    [id_solicitud] INT NOT NULL,
    CONSTRAINT [PK_casoorigensolicitud] PRIMARY KEY ([id_caso], [id_solicitud]),
    CONSTRAINT [casoorigensolicitud_ibfk_1] FOREIGN KEY ([id_caso]) REFERENCES [dbo].[caso] ([id_caso]),
    CONSTRAINT [casoorigensolicitud_ibfk_2] FOREIGN KEY ([id_solicitud]) REFERENCES [dbo].[solicitudacompanamiento] ([id_solicitud])
);

CREATE TABLE [dbo].[consentimientousuario] (
    [id_consentimiento] INT NOT NULL,
    [cedula_usuario] INT NOT NULL,
    [version_consentimiento] NVARCHAR(20) NOT NULL,
    [fecha_aceptacion] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_consentimientousuario] PRIMARY KEY ([id_consentimiento]),
    CONSTRAINT [consentimientousuario_ibfk_1] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula])
);

CREATE TABLE [dbo].[conversacionprofesional] (
    [id_conversacion_profesional] INT NOT NULL,
    [id_caso] INT NOT NULL,
    [fecha_inicio] DATETIME2(0) NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_conversacionprofesional] PRIMARY KEY ([id_conversacion_profesional]),
    CONSTRAINT [conversacionprofesional_ibfk_1] FOREIGN KEY ([id_caso]) REFERENCES [dbo].[caso] ([id_caso]),
    CONSTRAINT [conversacionprofesional_ibfk_2] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[rol] (
    [id_rol] INT NOT NULL,
    [nombre] NVARCHAR(50) NOT NULL,
    [descripcion] NVARCHAR(200) NOT NULL,
    CONSTRAINT [PK_rol] PRIMARY KEY ([id_rol]),
    CONSTRAINT [UQ_rol_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[empleadorol] (
    [cedula_empleado] INT NOT NULL,
    [id_rol] INT NOT NULL,
    CONSTRAINT [PK_empleadorol] PRIMARY KEY ([cedula_empleado], [id_rol]),
    CONSTRAINT [empleadorol_ibfk_1] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula]),
    CONSTRAINT [empleadorol_ibfk_2] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[rol] ([id_rol])
);

CREATE TABLE [dbo].[prefijotelefonico] (
    [id_prefijo] INT NOT NULL,
    [prefijo] SMALLINT NOT NULL,
    [pais] NVARCHAR(80) NOT NULL,
    CONSTRAINT [PK_prefijotelefonico] PRIMARY KEY ([id_prefijo]),
    CONSTRAINT [UQ_prefijotelefonico_prefijo] UNIQUE ([prefijo]),
    CONSTRAINT [UQ_prefijotelefonico_pais] UNIQUE ([pais])
);

CREATE TABLE [dbo].[tipotelefono] (
    [id_tipo_telefono] INT NOT NULL,
    [nombre] NVARCHAR(40) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_tipotelefono] PRIMARY KEY ([id_tipo_telefono]),
    CONSTRAINT [UQ_tipotelefono_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[telefono] (
    [id_telefono] INT NOT NULL,
    [numero] INT NOT NULL,
    [id_prefijo] INT NOT NULL,
    [id_tipo_telefono] INT NOT NULL,
    CONSTRAINT [PK_telefono] PRIMARY KEY ([id_telefono]),
    CONSTRAINT [telefono_ibfk_1] FOREIGN KEY ([id_prefijo]) REFERENCES [dbo].[prefijotelefonico] ([id_prefijo]),
    CONSTRAINT [telefono_ibfk_2] FOREIGN KEY ([id_tipo_telefono]) REFERENCES [dbo].[tipotelefono] ([id_tipo_telefono])
);

CREATE TABLE [dbo].[empleadotelefono] (
    [cedula_empleado] INT NOT NULL,
    [id_telefono] INT NOT NULL,
    CONSTRAINT [PK_empleadotelefono] PRIMARY KEY ([cedula_empleado], [id_telefono]),
    CONSTRAINT [empleadotelefono_ibfk_1] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula]),
    CONSTRAINT [empleadotelefono_ibfk_2] FOREIGN KEY ([id_telefono]) REFERENCES [dbo].[telefono] ([id_telefono])
);

CREATE TABLE [dbo].[mensajeempleadoprofesional] (
    [id_mensaje_empleado] INT NOT NULL,
    [id_conversacion_profesional] INT NOT NULL,
    [cedula_empleado] INT NOT NULL,
    [contenido] NVARCHAR(MAX) NOT NULL,
    [fecha_envio] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_mensajeempleadoprofesional] PRIMARY KEY ([id_mensaje_empleado]),
    CONSTRAINT [mensajeempleadoprofesional_ibfk_1] FOREIGN KEY ([id_conversacion_profesional]) REFERENCES [dbo].[conversacionprofesional] ([id_conversacion_profesional]),
    CONSTRAINT [mensajeempleadoprofesional_ibfk_2] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula])
);

CREATE TABLE [dbo].[mensajeusuarioprofesional] (
    [id_mensaje_usuario] INT NOT NULL,
    [id_conversacion_profesional] INT NOT NULL,
    [cedula_usuario] INT NOT NULL,
    [contenido] NVARCHAR(MAX) NOT NULL,
    [fecha_envio] DATETIME2(0) NOT NULL,
    CONSTRAINT [PK_mensajeusuarioprofesional] PRIMARY KEY ([id_mensaje_usuario]),
    CONSTRAINT [mensajeusuarioprofesional_ibfk_1] FOREIGN KEY ([id_conversacion_profesional]) REFERENCES [dbo].[conversacionprofesional] ([id_conversacion_profesional]),
    CONSTRAINT [mensajeusuarioprofesional_ibfk_2] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula])
);

CREATE TABLE [dbo].[tiporecurso] (
    [id_tipo_recurso] INT NOT NULL,
    [nombre] NVARCHAR(50) NOT NULL,
    [descripcion] NVARCHAR(200) NOT NULL,
    CONSTRAINT [PK_tiporecurso] PRIMARY KEY ([id_tipo_recurso]),
    CONSTRAINT [UQ_tiporecurso_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[recursoayuda] (
    [id_recurso] INT NOT NULL,
    [nombre] NVARCHAR(120) NOT NULL,
    [descripcion] NVARCHAR(MAX) NOT NULL,
    [id_tipo_recurso] INT NOT NULL,
    [id_estado_activo] INT NOT NULL,
    CONSTRAINT [PK_recursoayuda] PRIMARY KEY ([id_recurso]),
    CONSTRAINT [UQ_recursoayuda_nombre] UNIQUE ([nombre]),
    CONSTRAINT [recursoayuda_ibfk_1] FOREIGN KEY ([id_tipo_recurso]) REFERENCES [dbo].[tiporecurso] ([id_tipo_recurso]),
    CONSTRAINT [recursoayuda_ibfk_2] FOREIGN KEY ([id_estado_activo]) REFERENCES [dbo].[estadoactivo] ([id_estado_activo])
);

CREATE TABLE [dbo].[tipocontactorecurso] (
    [id_tipo_contacto] INT NOT NULL,
    [nombre] NVARCHAR(50) NOT NULL,
    [descripcion] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_tipocontactorecurso] PRIMARY KEY ([id_tipo_contacto]),
    CONSTRAINT [UQ_tipocontactorecurso_nombre] UNIQUE ([nombre])
);

CREATE TABLE [dbo].[recursocontacto] (
    [id_recurso_contacto] INT NOT NULL,
    [id_recurso] INT NOT NULL,
    [id_tipo_contacto] INT NOT NULL,
    [valor] NVARCHAR(300) NOT NULL,
    CONSTRAINT [PK_recursocontacto] PRIMARY KEY ([id_recurso_contacto]),
    CONSTRAINT [recursocontacto_ibfk_1] FOREIGN KEY ([id_recurso]) REFERENCES [dbo].[recursoayuda] ([id_recurso]),
    CONSTRAINT [recursocontacto_ibfk_2] FOREIGN KEY ([id_tipo_contacto]) REFERENCES [dbo].[tipocontactorecurso] ([id_tipo_contacto])
);

CREATE TABLE [dbo].[revisionalerta] (
    [id_revision_alerta] INT NOT NULL,
    [id_alerta] INT NOT NULL,
    [cedula_empleado] INT NOT NULL,
    [id_estado_alerta] INT NOT NULL,
    [fecha_revision] DATETIME2(0) NOT NULL,
    [observaciones] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_revisionalerta] PRIMARY KEY ([id_revision_alerta]),
    CONSTRAINT [revisionalerta_ibfk_1] FOREIGN KEY ([id_alerta]) REFERENCES [dbo].[alerta] ([id_alerta]),
    CONSTRAINT [revisionalerta_ibfk_2] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula]),
    CONSTRAINT [revisionalerta_ibfk_3] FOREIGN KEY ([id_estado_alerta]) REFERENCES [dbo].[estadoalerta] ([id_estado_alerta])
);

CREATE TABLE [dbo].[seguimiento] (
    [id_seguimiento] INT NOT NULL,
    [id_caso] INT NOT NULL,
    [cedula_empleado] INT NOT NULL,
    [fecha_seguimiento] DATETIME2(0) NOT NULL,
    [observaciones] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_seguimiento] PRIMARY KEY ([id_seguimiento]),
    CONSTRAINT [seguimiento_ibfk_1] FOREIGN KEY ([id_caso]) REFERENCES [dbo].[caso] ([id_caso]),
    CONSTRAINT [seguimiento_ibfk_2] FOREIGN KEY ([cedula_empleado]) REFERENCES [dbo].[empleado] ([cedula])
);

CREATE TABLE [dbo].[usuariotelefono] (
    [cedula_usuario] INT NOT NULL,
    [id_telefono] INT NOT NULL,
    CONSTRAINT [PK_usuariotelefono] PRIMARY KEY ([cedula_usuario], [id_telefono]),
    CONSTRAINT [usuariotelefono_ibfk_1] FOREIGN KEY ([cedula_usuario]) REFERENCES [dbo].[usuario] ([cedula]),
    CONSTRAINT [usuariotelefono_ibfk_2] FOREIGN KEY ([id_telefono]) REFERENCES [dbo].[telefono] ([id_telefono])
);

CREATE INDEX [IX_usuario_id_estado_activo] ON [dbo].[usuario] ([id_estado_activo]);
CREATE INDEX [IX_conversacionchatbot_cedula_usuario] ON [dbo].[conversacionchatbot] ([cedula_usuario]);
CREATE INDEX [IX_conversacionchatbot_id_estado_activo] ON [dbo].[conversacionchatbot] ([id_estado_activo]);
CREATE INDEX [IX_mensajechatbot_id_conversacion] ON [dbo].[mensajechatbot] ([id_conversacion]);
CREATE INDEX [IX_mensajechatbot_id_tipo_emisor] ON [dbo].[mensajechatbot] ([id_tipo_emisor]);
CREATE INDEX [IX_resultadoanalisis_id_mensaje] ON [dbo].[resultadoanalisis] ([id_mensaje]);
CREATE INDEX [IX_resultadoanalisis_id_nivel_riesgo] ON [dbo].[resultadoanalisis] ([id_nivel_riesgo]);
CREATE INDEX [IX_alerta_id_analisis] ON [dbo].[alerta] ([id_analisis]);
CREATE INDEX [IX_alerta_id_estado_alerta] ON [dbo].[alerta] ([id_estado_alerta]);
CREATE INDEX [IX_caso_cedula_usuario] ON [dbo].[caso] ([cedula_usuario]);
CREATE INDEX [IX_caso_id_estado_caso] ON [dbo].[caso] ([id_estado_caso]);
CREATE INDEX [IX_caso_id_estado_activo] ON [dbo].[caso] ([id_estado_activo]);
CREATE INDEX [IX_empleado_id_estado_activo] ON [dbo].[empleado] ([id_estado_activo]);
CREATE INDEX [IX_asignacioncaso_id_caso] ON [dbo].[asignacioncaso] ([id_caso]);
CREATE INDEX [IX_asignacioncaso_cedula_empleado] ON [dbo].[asignacioncaso] ([cedula_empleado]);
CREATE INDEX [IX_asignacioncaso_id_estado_asignacion] ON [dbo].[asignacioncaso] ([id_estado_asignacion]);
CREATE INDEX [IX_auditoriaacceso_cedula_empleado] ON [dbo].[auditoriaacceso] ([cedula_empleado]);
CREATE INDEX [IX_auditoriaacceso_id_tipo_recurso_auditable] ON [dbo].[auditoriaacceso] ([id_tipo_recurso_auditable]);
CREATE INDEX [IX_casofactorriesgo_id_factor_riesgo] ON [dbo].[casofactorriesgo] ([id_factor_riesgo]);
CREATE INDEX [IX_solicitudacompanamiento_cedula_usuario] ON [dbo].[solicitudacompanamiento] ([cedula_usuario]);
CREATE INDEX [IX_solicitudacompanamiento_id_estado_solicitud] ON [dbo].[solicitudacompanamiento] ([id_estado_solicitud]);
CREATE INDEX [IX_casoorigensolicitud_id_solicitud] ON [dbo].[casoorigensolicitud] ([id_solicitud]);
CREATE INDEX [IX_consentimientousuario_cedula_usuario] ON [dbo].[consentimientousuario] ([cedula_usuario]);
CREATE INDEX [IX_conversacionprofesional_id_caso] ON [dbo].[conversacionprofesional] ([id_caso]);
CREATE INDEX [IX_conversacionprofesional_id_estado_activo] ON [dbo].[conversacionprofesional] ([id_estado_activo]);
CREATE INDEX [IX_empleadorol_id_rol] ON [dbo].[empleadorol] ([id_rol]);
CREATE INDEX [IX_telefono_id_prefijo] ON [dbo].[telefono] ([id_prefijo]);
CREATE INDEX [IX_telefono_id_tipo_telefono] ON [dbo].[telefono] ([id_tipo_telefono]);
CREATE INDEX [IX_empleadotelefono_id_telefono] ON [dbo].[empleadotelefono] ([id_telefono]);
CREATE INDEX [IX_mensajeempleadoprofesional_id_conversacion_profesional] ON [dbo].[mensajeempleadoprofesional] ([id_conversacion_profesional]);
CREATE INDEX [IX_mensajeempleadoprofesional_cedula_empleado] ON [dbo].[mensajeempleadoprofesional] ([cedula_empleado]);
CREATE INDEX [IX_mensajeusuarioprofesional_id_conversacion_profesional] ON [dbo].[mensajeusuarioprofesional] ([id_conversacion_profesional]);
CREATE INDEX [IX_mensajeusuarioprofesional_cedula_usuario] ON [dbo].[mensajeusuarioprofesional] ([cedula_usuario]);
CREATE INDEX [IX_recursoayuda_id_tipo_recurso] ON [dbo].[recursoayuda] ([id_tipo_recurso]);
CREATE INDEX [IX_recursoayuda_id_estado_activo] ON [dbo].[recursoayuda] ([id_estado_activo]);
CREATE INDEX [IX_recursocontacto_id_recurso] ON [dbo].[recursocontacto] ([id_recurso]);
CREATE INDEX [IX_recursocontacto_id_tipo_contacto] ON [dbo].[recursocontacto] ([id_tipo_contacto]);
CREATE INDEX [IX_revisionalerta_id_alerta] ON [dbo].[revisionalerta] ([id_alerta]);
CREATE INDEX [IX_revisionalerta_cedula_empleado] ON [dbo].[revisionalerta] ([cedula_empleado]);
CREATE INDEX [IX_revisionalerta_id_estado_alerta] ON [dbo].[revisionalerta] ([id_estado_alerta]);
CREATE INDEX [IX_seguimiento_id_caso] ON [dbo].[seguimiento] ([id_caso]);
CREATE INDEX [IX_seguimiento_cedula_empleado] ON [dbo].[seguimiento] ([cedula_empleado]);
CREATE INDEX [IX_usuariotelefono_id_telefono] ON [dbo].[usuariotelefono] ([id_telefono]);
GO
