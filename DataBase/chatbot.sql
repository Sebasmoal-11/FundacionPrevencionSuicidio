USE [fundacion_prevencion];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Estos emisores corresponden a los roles que utiliza el historial de Gemini.
IF NOT EXISTS (SELECT 1 FROM dbo.tipoemisorchatbot WHERE id_tipo_emisor = 1)
    INSERT dbo.tipoemisorchatbot (id_tipo_emisor, nombre, descripcion)
    VALUES (1, N'Usuario', N'Mensaje enviado por el usuario');

IF NOT EXISTS (SELECT 1 FROM dbo.tipoemisorchatbot WHERE id_tipo_emisor = 2)
    INSERT dbo.tipoemisorchatbot (id_tipo_emisor, nombre, descripcion)
    VALUES (2, N'Chatbot', N'Respuesta del asistente virtual');

IF OBJECT_ID(N'dbo.Seq_ConversacionChatbot', N'SO') IS NULL
BEGIN
    DECLARE @conversacion BIGINT;
    SELECT @conversacion = COALESCE(MAX(CONVERT(BIGINT, id_conversacion)), 0) + 1
    FROM dbo.conversacionchatbot WITH (TABLOCKX, HOLDLOCK);

    DECLARE @sql_conversacion NVARCHAR(200) =
        N'CREATE SEQUENCE dbo.Seq_ConversacionChatbot AS INT START WITH ' +
        CONVERT(NVARCHAR(20), @conversacion) + N' INCREMENT BY 1';
    EXEC sys.sp_executesql @sql_conversacion;
END;

IF OBJECT_ID(N'dbo.Seq_MensajeChatbot', N'SO') IS NULL
BEGIN
    DECLARE @mensaje BIGINT;
    SELECT @mensaje = COALESCE(MAX(CONVERT(BIGINT, id_mensaje)), 0) + 1
    FROM dbo.mensajechatbot WITH (TABLOCKX, HOLDLOCK);

    DECLARE @sql_mensaje NVARCHAR(200) =
        N'CREATE SEQUENCE dbo.Seq_MensajeChatbot AS INT START WITH ' +
        CONVERT(NVARCHAR(20), @mensaje) + N' INCREMENT BY 1';
    EXEC sys.sp_executesql @sql_mensaje;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.conversacionchatbot')
                 AND name = N'id_conversacion' AND (default_object_id <> 0 OR is_identity = 1))
    ALTER TABLE dbo.conversacionchatbot ADD CONSTRAINT DF_conversacionchatbot_id
        DEFAULT (NEXT VALUE FOR dbo.Seq_ConversacionChatbot) FOR id_conversacion;

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.mensajechatbot')
                 AND name = N'id_mensaje' AND (default_object_id <> 0 OR is_identity = 1))
    ALTER TABLE dbo.mensajechatbot ADD CONSTRAINT DF_mensajechatbot_id
        DEFAULT (NEXT VALUE FOR dbo.Seq_MensajeChatbot) FOR id_mensaje;

COMMIT;
GO
