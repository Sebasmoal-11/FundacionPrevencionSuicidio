/*
    Sincroniza la base de datos con el modelo de EF Core del módulo de alertas
    y del chatbot (commit 080501a).

    Problema que corrige:
      - El modelo mapea conversacionchatbot.oculta_usuario, pero la columna no
        existe en la tabla.
      - El modelo espera que alerta.id_alerta se genere con dbo.Seq_Alerta.
      - El modelo espera que resultadoanalisis.id_analisis se genere con
        dbo.Seq_ResultadoAnalisis.

    Qué hace:
      1. Agrega conversacionchatbot.oculta_usuario BIT NOT NULL DEFAULT 0.
      2. Crea dbo.Seq_Alerta y la usa como DEFAULT de alerta.id_alerta.
      3. Crea dbo.Seq_ResultadoAnalisis y la usa como DEFAULT de
         resultadoanalisis.id_analisis.

    Es idempotente: se puede ejecutar varias veces sin duplicar nada.
    No elimina ni modifica datos existentes.

    Uso (ajusta la instancia si tu app usa otra):
      sqlcmd -S "localhost" -d fundacion_prevencion -E -C -i DataBase/alertas_chatbot.sql

    Si usas la instancia de appsettings.json:
      sqlcmd -S "localhost\SQLEXPRESS" -d fundacion_prevencion -E -C -i DataBase/alertas_chatbot.sql

    En una base nueva, ejecútalo después de DataBase/chatbot.sql.
*/

USE [fundacion_prevencion];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* 1. Columna para ocultar conversaciones del historial. */
IF COL_LENGTH(N'dbo.conversacionchatbot', N'oculta_usuario') IS NULL
BEGIN
    ALTER TABLE dbo.conversacionchatbot
        ADD oculta_usuario BIT NOT NULL
            CONSTRAINT DF_conversacionchatbot_oculta DEFAULT (0);
END;

/* 2. Secuencia y DEFAULT para alerta.id_alerta. */
IF OBJECT_ID(N'dbo.Seq_Alerta', N'SO') IS NULL
BEGIN
    DECLARE @inicio_alerta BIGINT;

    SELECT @inicio_alerta = COALESCE(MAX(CONVERT(BIGINT, id_alerta)), 0) + 1
    FROM dbo.alerta WITH (TABLOCKX, HOLDLOCK);

    DECLARE @sql_alerta NVARCHAR(200) =
        N'CREATE SEQUENCE dbo.Seq_Alerta AS INT START WITH ' +
        CONVERT(NVARCHAR(20), @inicio_alerta) + N' INCREMENT BY 1';

    EXEC sys.sp_executesql @sql_alerta;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    JOIN sys.columns c
      ON c.object_id = dc.parent_object_id
     AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.alerta')
      AND c.name = N'id_alerta')
BEGIN
    -- Se usa SQL dinámico para que la secuencia ya exista al compilar el ALTER.
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.alerta
            ADD CONSTRAINT DF_alerta_id
                DEFAULT (NEXT VALUE FOR dbo.Seq_Alerta) FOR id_alerta;';
END;

/* 3. Secuencia y DEFAULT para resultadoanalisis.id_analisis. */
IF OBJECT_ID(N'dbo.Seq_ResultadoAnalisis', N'SO') IS NULL
BEGIN
    DECLARE @inicio_analisis BIGINT;

    SELECT @inicio_analisis = COALESCE(MAX(CONVERT(BIGINT, id_analisis)), 0) + 1
    FROM dbo.resultadoanalisis WITH (TABLOCKX, HOLDLOCK);

    DECLARE @sql_analisis NVARCHAR(200) =
        N'CREATE SEQUENCE dbo.Seq_ResultadoAnalisis AS INT START WITH ' +
        CONVERT(NVARCHAR(20), @inicio_analisis) + N' INCREMENT BY 1';

    EXEC sys.sp_executesql @sql_analisis;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    JOIN sys.columns c
      ON c.object_id = dc.parent_object_id
     AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.resultadoanalisis')
      AND c.name = N'id_analisis')
BEGIN
    -- Se usa SQL dinámico para que la secuencia ya exista al compilar el ALTER.
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.resultadoanalisis
            ADD CONSTRAINT DF_resultadoanalisis_id
                DEFAULT (NEXT VALUE FOR dbo.Seq_ResultadoAnalisis) FOR id_analisis;';
END;

COMMIT;
GO

PRINT N'Esquema de alertas/chatbot actualizado correctamente.';
GO
