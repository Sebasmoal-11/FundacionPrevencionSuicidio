using System.ComponentModel.DataAnnotations;

namespace Proyecto.Autenticacion;

public static class AltaPersonal
{
    public static async Task Ejecutar(IServiceProvider servicios)
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("Ejecute el alta en una terminal interactiva.");

        var formulario = new RegistroForm
        {
            Cedula = Leer("Cédula"),
            Nombre = Leer("Nombre"),
            PrimerApellido = Leer("Primer apellido"),
            SegundoApellido = Leer("Segundo apellido"),
            Correo = Leer("Correo"),
            Contrasena = LeerClave("Contraseña"),
            Confirmacion = LeerClave("Confirmar contraseña")
        };

        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(formulario, new ValidationContext(formulario), errores, true)
            || !int.TryParse(formulario.Cedula, out int cedula) || cedula <= 0)
        {
            Console.Error.WriteLine("Revise la cédula y los datos de la cuenta.");
            foreach (var error in errores)
                Console.Error.WriteLine(error.ErrorMessage);
            Environment.ExitCode = 1;
            return;
        }

        await using var alcance = servicios.CreateAsyncScope();
        var servicio = alcance.ServiceProvider.GetRequiredService<Cuentas>();
        var mensaje = await servicio.Crear(formulario, cedula, empleado: true);
        if (mensaje is null)
            Console.WriteLine("Cuenta de personal creada.");
        else
        {
            Console.Error.WriteLine(mensaje);
            Environment.ExitCode = 1;
        }
    }

    private static string Leer(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    private static string LeerClave(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        var texto = new System.Text.StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
                break;
            if (tecla.Key == ConsoleKey.Backspace && texto.Length > 0)
                texto.Length--;
            else if (!char.IsControl(tecla.KeyChar))
                texto.Append(tecla.KeyChar);
        }
        Console.WriteLine();
        return texto.ToString();
    }
}
