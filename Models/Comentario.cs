namespace tp_redSocial.Models;

using System;

public class Comentario
{
    public int Id { get; set; }
    public int IdPublicacion { get; set; }
    public int IdUsuarioComenta { get; set; }
    public string Texto { get; set; }
    public DateTime FechaComentario { get; set; }

    public string NombreUsuario { get; set; }

    public static bool ValidarDatosComentario(string texto)
    {
        bool esValido = true;

        if (string.IsNullOrWhiteSpace(texto))
        {
            esValido = false;
        }

        return esValido;
    }
}