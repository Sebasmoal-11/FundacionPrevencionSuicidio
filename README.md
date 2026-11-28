# Fundación para la Prevención del Suicidio

La solución usa ASP.NET Core MVC (.NET 9), Razor, Entity Framework Core y SQL Server Express. El proyecto de inicio es `Proyecto/FundacionPrevencion.API.csproj`.

## Base de datos

En SSMS, ejecuta estos scripts en orden sobre la misma instancia:

1. `DataBase/fundacion_prevencion.sql` para una base nueva.
2. `DataBase/autenticacion.sql` para el estado activo y las sesiones revocables.
3. `DataBase/chatbot.sql` para los emisores y los IDs de conversaciones y mensajes.
4. `DataBase/administracion.sql` para los estados de las alertas.
5. `DataBase/casos.sql` para la trazabilidad alerta-caso y los catálogos de casos.

Usa una cuenta con acceso a las tablas de autenticación, del chatbot y de casos. Si reutilizas el usuario SQL de la aplicación anterior, también necesita actualizar `contrasena_hash` en `usuario` y `empleado` para actualizar hashes antiguos al iniciar sesión. El servicio conserva el formato scrypt estándar que generaba el módulo anterior y lo actualiza a PBKDF2 de ASP.NET tras un acceso válido. Las sesiones anteriores requieren volver a iniciar sesión.

## Ejecutar en local

Configura la conexión y la clave de Gemini mediante secretos del proyecto en Visual Studio, o mediante las variables de entorno `ConnectionStrings__ConexionFundacion` y `Gemini__ApiKey`. Si usas tu SQL Server Express local con autenticación de Windows y el certificado de desarrollo, la cadena puede ser:

```text
Server=localhost\SQLEXPRESS;Database=fundacion_prevencion;Trusted_Connection=True;TrustServerCertificate=True
```

Desde la raíz, ejecuta:

```powershell
dotnet run --project Proyecto/FundacionPrevencion.API.csproj --launch-profile https
```

Abre la URL que indica la terminal. Las rutas de acceso son `/ingreso`, `/registro` y `/personal/ingreso`. Desde **Mi cuenta**, el usuario puede abrir el chatbot. La vista del chatbot y su API requieren una sesión de usuario. El servidor obtiene la cédula desde la identidad autenticada y la capa de datos verifica que cada conversación pertenezca a esa cuenta.

Para crear un empleado en una terminal sin publicar sus credenciales:

```powershell
dotnet run --project Proyecto/FundacionPrevencion.API.csproj -- --crear-empleado
```

La conexión usada para el alta necesita insertar en `empleado`, `rol` y `empleadorol`. El comando solicita la contraseña sin mostrarla y asigna el rol `Personal`.

## Comprobar la integración

1. Registra dos usuarios ficticios y comprueba las validaciones y el rechazo de duplicados.
2. Inicia sesión con el primero, abre el chatbot y crea una conversación.
3. Cierra sesión. Comprueba que el chatbot pida iniciar sesión de nuevo.
4. Entra con el segundo usuario y verifica que su historial esté separado del primero.
5. Entra como empleado con rol `Personal` y verifica que se abra el área del personal y que el chatbot de usuarios quede restringido.
