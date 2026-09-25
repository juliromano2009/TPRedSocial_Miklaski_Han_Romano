namespace tp_redSocial.Models;

using System.Text.RegularExpressions;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
    public string Apellido { get; set; }
    public string TipoUsuario { get; set; }

    public static bool ValidarDatosRegistro(string nombre, string apellido, string nombreUsuario, string contraseña)
    {
        bool esValido = true;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            esValido = false;
        }
        else if (!Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(apellido))
        {
            esValido = false;
        }
        else if (!Regex.IsMatch(apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            esValido = false;
        }
        else if (nombreUsuario.Length < 6)
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(contraseña))
        {
            esValido = false;
        }
        else if (contraseña.Length <= 8)
        {
            esValido = false;
        }

        return esValido;
    }
}