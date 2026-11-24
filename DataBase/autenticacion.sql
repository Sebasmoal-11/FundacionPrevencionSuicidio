USE [fundacion_prevencion];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.estadoactivo WHERE nombre = N'Activo')
BEGIN
    DECLARE @id INT;

    SELECT @id = COALESCE(MAX(id_estado_activo), 0) + 1
    FROM dbo.estadoactivo WITH (UPDLOCK, HOLDLOCK);

    INSERT dbo.estadoactivo (id_estado_activo, nombre, descripcion)
    VALUES (@id, N'Activo', N'Cuenta habilitada');
END;

IF OBJECT_ID(N'dbo.sesionautenticacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.sesionautenticacion (
        token_hash CHAR(64) NOT NULL PRIMARY KEY,
        cedula_usuario INT NULL REFERENCES dbo.usuario(cedula) ON DELETE CASCADE,
        cedula_empleado INT NULL REFERENCES dbo.empleado(cedula) ON DELETE CASCADE,
        vence DATETIME2(0) NOT NULL,
        CONSTRAINT CK_sesionautenticacion_cuenta CHECK (
            (cedula_usuario IS NOT NULL AND cedula_empleado IS NULL) OR
            (cedula_usuario IS NULL AND cedula_empleado IS NOT NULL)
        )
    );

    CREATE INDEX IX_sesionautenticacion_vence
        ON dbo.sesionautenticacion (vence);
END;

COMMIT;
GO