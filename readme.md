# Fundación para la Prevención del Suicidio

## Módulo de usuarios y autenticación

Este módulo permite registrar usuarios, iniciar y cerrar sesión y controlar el acceso según el tipo de cuenta. El registro público crea cuentas de usuario. Las cuentas del personal se crean mediante un comando administrativo y reciben el rol `Personal`.

La aplicación usa Flask y SQL Server Express. `app.py` contiene los controladores y la protección de rutas, `cuentas.py` gestiona el acceso a datos y `templates/` y `static/` contienen la interfaz. Las contraseñas se almacenan con hash scrypt y las sesiones se registran en SQL Server para poder invalidarlas al cerrar sesión.

## Requisitos

- Python 3.12 como versión de referencia.
- SQL Server Express y SQL Server Management Studio (SSMS).
- Una instancia local de SQL Server con autenticación mixta y TCP habilitado.

## Configurar SQL Server Express para pruebas locales

Conéctate desde SSMS a `localhost\SQLEXPRESS` con **Windows Authentication**. Si tu instancia tiene otro nombre, usa el que mostró el instalador.

### Activar la autenticación mixta

1. En **Object Explorer**, haz clic derecho sobre el servidor y abre **Properties → Security**.
2. Selecciona **SQL Server and Windows Authentication mode** y pulsa **OK**. Esto permite que Flask se conecte con el usuario `fundacion_app`.

### Configurar el puerto TCP local

1. Abre **SQL Server Configuration Manager** y ve a **SQL Server Network Configuration → Protocols for SQLEXPRESS**.
2. Habilita **TCP/IP** y abre sus propiedades. En la pestaña **Protocol**, configura **Listen All** en **No**.
3. En **IP Addresses**, busca el bloque cuya dirección sea `127.0.0.1`. Configura **Enabled** en **Yes**, deja **TCP Dynamic Ports** vacío y escribe `14330` en **TCP Port**.
4. Guarda los cambios. Abre **Servicios de Windows** con `services.msc` y reinicia **SQL Server (SQLEXPRESS)** para aplicar ambos cambios.

Vuelve a conectarte en SSMS y comprueba el modo de autenticación:

```sql
SELECT SERVERPROPERTY('IsIntegratedSecurityOnly') AS solo_autenticacion_windows;
```

El resultado debe ser `0`. Después, en PowerShell, comprueba el puerto:

```powershell
Test-NetConnection 127.0.0.1 -Port 14330
```

Cuando aparezca `TcpTestSucceeded : True`, continúa con **Preparar la base de datos**.

## Preparar la base de datos

1. Conéctate en SSMS con una cuenta autorizada para administrar la instancia.
2. Si la base `fundacion_prevencion` todavía no existe, ejecuta `DataBase/fundacion_prevencion.sql`.
3. Ejecuta `DataBase/autenticacion.sql`. El script agrega el estado `Activo` si hace falta y crea la tabla de sesiones. Se puede ejecutar nuevamente.
4. Crea un inicio de sesión de SQL Server llamado `fundacion_app` y asígnale un usuario con el mismo nombre en la base `fundacion_prevencion`. Usa una contraseña local.

Con el usuario ya creado en la base, aplica estos permisos desde SSMS:

```sql
USE [fundacion_prevencion];

GRANT SELECT, INSERT ON dbo.persona TO [fundacion_app];
GRANT SELECT, INSERT ON dbo.usuario TO [fundacion_app];
GRANT SELECT ON dbo.empleado TO [fundacion_app];
GRANT SELECT ON dbo.estadoactivo TO [fundacion_app];
GRANT SELECT ON dbo.rol TO [fundacion_app];
GRANT SELECT ON dbo.empleadorol TO [fundacion_app];
GRANT SELECT, INSERT, DELETE ON dbo.sesionautenticacion TO [fundacion_app];
```

La configuración de prueba utiliza SQL Server en `127.0.0.1:14330`. Si tu instancia usa otro puerto, ajusta `SQL_PORT` en los comandos de abajo. Puedes comprobar la conexión TCP desde PowerShell con `Test-NetConnection 127.0.0.1 -Port 14330`.

## Ejecutar en Windows

Desde la raíz del repositorio, en PowerShell:

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Configura las variables y arranca Flask en la **misma terminal**:

```powershell
$env:SQL_HOST = "127.0.0.1"
$env:SQL_PORT = "14330"
$env:SQL_DATABASE = "fundacion_prevencion"
$env:SQL_USER = "fundacion_app"
$clave = Read-Host "Contraseña de fundacion_app" -AsSecureString
$env:SQL_PASSWORD = [System.Net.NetworkCredential]::new("fundacion_app", $clave).Password

$env:AUTH_SECRET = (.\.venv\Scripts\python.exe -c "import secrets; print(secrets.token_urlsafe(48))")
$env:AUTH_LOCAL_HTTP = "1"

.\.venv\Scripts\python.exe -m flask --app app run
```

Abre <http://127.0.0.1:5000>. Si abres otra terminal, tendrás que configurar sus variables de entorno antes de ejecutar Flask. Usa `AUTH_LOCAL_HTTP=1` únicamente para pruebas locales. Mantén `SQL_PASSWORD` y `AUTH_SECRET` fuera del repositorio.

## Rutas de acceso

| Ruta | Función |
| --- | --- |
| `/registro` | Registro público de usuarios |
| `/ingreso` | Inicio de sesión de usuarios |
| `/personal/ingreso` | Inicio de sesión del personal |
| `/cuenta` | Área protegida para usuarios |
| `/personal` | Área protegida para empleados con rol `Personal` |
| `POST /salir` | Cierre de sesión desde el encabezado |

## Crear un empleado de prueba

El alta requiere permisos adicionales de `INSERT` sobre `dbo.empleado`, `dbo.rol` y `dbo.empleadorol`. En el entorno local, un administrador puede concederlos temporalmente a `fundacion_app` y revocarlos al terminar.

En la terminal configurada, detén Flask con **Ctrl+C** si está corriendo y ejecuta:

```powershell
.\.venv\Scripts\python.exe -m flask --app app crear-empleado
```

Introduce datos ficticios y una cédula distinta de la del usuario de prueba. El comando asigna el rol `Personal`. Después de crear la cuenta, revoca los permisos temporales, vuelve a arrancar Flask y entra desde `/personal/ingreso`.

## Pruebas manuales

1. Registra un usuario ficticio. Comprueba la confirmación y verifica que el sistema rechace el registro duplicado.
2. Intenta iniciar sesión con una contraseña incorrecta. Después usa la correcta y comprueba que se abra `/cuenta`.
3. Con la sesión de usuario abierta, visita `/personal`. Debe denegar el acceso.
4. Cierra sesión y vuelve a abrir `/cuenta`. Debe redirigir al inicio de sesión.
5. Entra como empleado. Comprueba que `/personal` muestre el rol `Personal` y que `/cuenta` deniegue el acceso.

Estos flujos se probaron en una instalación local de SQL Server Express.

## Integración con los demás módulos

Las nuevas rutas protegidas pueden reutilizar `requiere_permiso` de `app.py`. Usa `@requiere_permiso("area_usuario")` para rutas de usuarios y `@requiere_permiso("area_personal")` para rutas del personal autorizado. Una vez validada la sesión, `g.cuenta` contiene la cédula, el tipo de cuenta y los roles. Cada módulo debe comprobar además si esa cuenta puede acceder al recurso concreto, como una conversación o un caso.