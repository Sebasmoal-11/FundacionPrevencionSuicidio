/*
    Catálogos administrativos de la Fundación.

    Este script es aditivo e idempotente: se puede ejecutar varias veces sin
    duplicar registros ni modificar los que ya existen.

    Contenido actual: estados de alerta del módulo de gestión de alertas.
    Los estados de casos, asignaciones y solicitudes se agregarán junto con
    sus respectivos módulos.
*/

USE [fundacion_prevencion];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.estadoalerta', N'U') IS NULL
    THROW 50001, 'Falta la tabla estadoalerta. Ejecute primero fundacion_prevencion.sql.', 1;

DECLARE @id INT;

IF NOT EXISTS (SELECT 1 FROM dbo.estadoalerta WHERE nombre = N'Pendiente de revisión')
BEGIN
    SELECT @id = COALESCE(MAX(id_estado_alerta), 0) + 1
    FROM dbo.estadoalerta WITH (UPDLOCK, HOLDLOCK);

    INSERT dbo.estadoalerta (id_estado_alerta, nombre, descripcion)
    VALUES (@id, N'Pendiente de revisión',
            N'Alerta generada que aún no ha sido revisada por el personal.');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.estadoalerta WHERE nombre = N'En revisión')
BEGIN
    SELECT @id = COALESCE(MAX(id_estado_alerta), 0) + 1
    FROM dbo.estadoalerta WITH (UPDLOCK, HOLDLOCK);

    INSERT dbo.estadoalerta (id_estado_alerta, nombre, descripcion)
    VALUES (@id, N'En revisión',
            N'Alerta que está siendo evaluada por el personal.');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.estadoalerta WHERE nombre = N'Cerrada')
BEGIN
    SELECT @id = COALESCE(MAX(id_estado_alerta), 0) + 1
    FROM dbo.estadoalerta WITH (UPDLOCK, HOLDLOCK);

    INSERT dbo.estadoalerta (id_estado_alerta, nombre, descripcion)
    VALUES (@id, N'Cerrada',
            N'Alerta revisada y finalizada.');
END;

COMMIT;
GO
