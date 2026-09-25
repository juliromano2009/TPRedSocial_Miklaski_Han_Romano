namespace tp_redSocial.Models;

using System;

public class Publicacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Imagen { get; set; }
    public DateTime FechaPublicacion { get; set; }

    public string NombreUsuario { get; set; }
    public int CantidadMeGusta { get; set; }
    public bool UsuarioActualDioMeGusta { get; set; }
    public List<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public static bool ValidarDatosPublicacion(string titulo, string descripcion, string imagen)
    {
        bool esValido = true;

        if (string.IsNullOrWhiteSpace(titulo))
        {
            esValido = false;
        }
        else if (titulo.Length > 200)
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(imagen))
        {
            esValido = false;
        }

        return esValido;
    }
}