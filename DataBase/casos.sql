/*
    Módulo de gestión de casos.

    Este script es aditivo e idempotente: se puede ejecutar varias veces sin
    duplicar la tabla ni los catálogos. No modifica datos existentes.

    Contenido:
      1. Tabla intermedia alertacaso (trazabilidad alerta -> caso).
      2. Semilla inicial de estadocaso, solo si la tabla está vacía.
      3. Semilla inicial de estadoasignacion, solo si la tabla está vacía.

    IMPORTANTE:
      - Ejecutar manualmente después de fundacion_prevencion.sql.
      - Los nombres de los estados son una propuesta inicial. Revíselos y
        ajústelos antes de ejecutar si la Fundación usa otra nomenclatura.
        El código no depende de estos nombres: solo valida que el id enviado
        exista en el catálogo correspondiente.
      - Si las tablas estadocaso o estadoasignacion ya tienen filas, este
        script no inserta nada y se conservan los ids existentes.
*/

USE [fundacion_prevencion];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.caso', N'U') IS NULL
    OR OBJECT_ID(N'dbo.alerta', N'U') IS NULL
    OR OBJECT_ID(N'dbo.estadocaso', N'U') IS NULL
    OR OBJECT_ID(N'dbo.estadoasignacion', N'U') IS NULL
    THROW 50002, 'Faltan tablas base. Ejecute primero fundacion_prevencion.sql.', 1;

/* 1. Trazabilidad del origen del caso. */
IF OBJECT_ID(N'dbo.alertacaso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.alertacaso (
        id_alerta_caso INT NOT NULL,
        id_alerta INT NOT NULL,
        id_caso INT NOT NULL,
        CONSTRAINT PK_alertacaso PRIMARY KEY (id_alerta_caso),
        CONSTRAINT UQ_alertacaso_id_alerta UNIQUE (id_alerta),
        CONSTRAINT alertacaso_ibfk_1 FOREIGN KEY (id_alerta)
            REFERENCES dbo.alerta (id_alerta),
        CONSTRAINT alertacaso_ibfk_2 FOREIGN KEY (id_caso)
            REFERENCES dbo.caso (id_caso)
    );

    CREATE INDEX IX_alertacaso_id_caso
        ON dbo.alertacaso (id_caso);
END;

/* 2. Estados del caso (solo si el catálogo está vacío). */
IF NOT EXISTS (SELECT 1 FROM dbo.estadocaso)
BEGIN
    INSERT dbo.estadocaso (id_estado_caso, nombre, descripcion)
    VALUES
        (1, N'Abierto', N'Caso registrado y pendiente de seguimiento.'),
        (2, N'En seguimiento', N'Caso con acompañamiento en curso.'),
        (3, N'Cerrado', N'Caso finalizado.');
END;

/* 3. Estados de la asignación (solo si el catálogo está vacío). */
IF NOT EXISTS (SELECT 1 FROM dbo.estadoasignacion)
BEGIN
    INSERT dbo.estadoasignacion (id_estado_asignacion, nombre, descripcion)
    VALUES
        (1, N'Activa', N'Asignación vigente del caso.'),
        (2, N'Finalizada', N'Asignación concluida.');
END;

COMMIT;
GO
