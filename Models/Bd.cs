namespace tp_redSocial.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;

public class Bd
{
    // OJO: estaba como campo de instancia, pero todos los métodos son static,
    // así que tiene que ser static también (si no, no compila)
    private static string _connectionString = @"Server=localhost;Database=DBRedSocial; Integrated Security=True; TrustServerCertificate=True;";

    // ---------------- Usuarios ----------------

    public static void AgregarUsuario(Usuario usuario)
    {
        string nombre = usuario.Nombre;
        string nombreUsuario = usuario.NombreUsuario;
        string contraseña = usuario.Contraseña;
        string apellido = usuario.Apellido;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido)
                             VALUES (@nombreUsuario, @contraseña, @nombre, @apellido)";
            connection.Execute(query, new
            {
                NombreUsuario = nombreUsuario,
                Contraseña = contraseña,
                Nombre = nombre,
                Apellido = apellido,
            });
        }
    }

    public static bool FijarseSiExisteUsuario(string nombreUsuario)
    {
        bool existe = false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @nombreUsuario";

            int count = connection.QueryFirstOrDefault<int>(query, new { NombreUsuario = nombreUsuario });

            if (count > 0)
            {
                existe = true;
            }
            else
            {
                existe = false;
            }
        }
        return existe;
    }

    public static Usuario ObtenerUsuario(string nombreUsuario, string contraseña)
    {
        Usuario usuario = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuarios WHERE NombreUsuario = @nombreUsuario AND Contraseña = @contraseña";
            usuario = connection.QueryFirstOrDefault<Usuario>(query, new { nombreUsuario, contraseña });
        }
        return usuario;
    }

    // Hace falta para recuperar los datos del usuario logueado a partir del Id guardado en Session
    public static Usuario ObtenerUsuarioPorId(int id)
    {
        Usuario usuario = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuarios WHERE Id = @id";
            usuario = connection.QueryFirstOrDefault<Usuario>(query, new { id });
        }
        return usuario;
    }

    // ---------------- Publicaciones ----------------

    public static void AgregarPublicacion(Publicacion publicacion)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion)
                             VALUES (@idUsuario, @titulo, @descripcion, @imagen, @fechaPublicacion)";
            connection.Execute(query, new
            {
                idUsuario = publicacion.IdUsuario,
                titulo = publicacion.Titulo,
                descripcion = publicacion.Descripcion,
                imagen = publicacion.Imagen,
                fechaPublicacion = publicacion.FechaPublicacion,
            });
        }
    }

    public static Publicacion ObtenerPublicacionPorId(int idPublicacion)
    {
        Publicacion publicacion = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Publicaciones WHERE Id = @idPublicacion";
            publicacion = connection.QueryFirstOrDefault<Publicacion>(query, new { idPublicacion });
        }
        return publicacion;
    }

    // Trae "cantidad" publicaciones a partir de "desde", ordenadas de más nueva a más vieja,
    // ya con NombreUsuario, CantidadMeGusta y si el usuario logueado le dio Me Gusta
    public static List<Publicacion> ObtenerPublicaciones(int desde, int cantidad, int idUsuarioActual)
    {
        List<Publicacion> publicaciones;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT p.Id, p.IdUsuario, p.Titulo, p.Descripcion, p.Imagen, p.FechaPublicacion,
                       u.NombreUsuario
                FROM Publicaciones p
                INNER JOIN Usuarios u ON u.Id = p.IdUsuario
                ORDER BY p.FechaPublicacion DESC
                OFFSET @desde ROWS FETCH NEXT @cantidad ROWS ONLY";

            publicaciones = connection.Query<Publicacion, string, Publicacion>(
                query,
                (publicacion, nombreUsuario) =>
                {
                    publicacion.NombreUsuario = nombreUsuario;
                    return publicacion;
                },
                new { desde, cantidad },
                splitOn: "NombreUsuario").ToList();

            foreach (Publicacion publicacion in publicaciones)
            {
                publicacion.CantidadMeGusta = ContarMeGusta(publicacion.Id);
                publicacion.UsuarioActualDioMeGusta = FijarseSiExisteMeGusta(publicacion.Id, idUsuarioActual);
                publicacion.Comentarios = ObtenerComentarios(publicacion.Id);
            }
        }

        return publicaciones;
    }

    // ---------------- Comentarios ----------------

    public static Comentario AgregarComentario(Comentario comentario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario)
                             OUTPUT INSERTED.Id
                             VALUES (@idPublicacion, @idUsuarioComenta, @texto, @fechaComentario)";

            int idNuevoComentario = connection.QuerySingle<int>(query, new
            {
                idPublicacion = comentario.IdPublicacion,
                idUsuarioComenta = comentario.IdUsuarioComenta,
                texto = comentario.Texto,
                fechaComentario = comentario.FechaComentario,
            });

            comentario.Id = idNuevoComentario;
        }
        return comentario;
    }

    public static List<Comentario> ObtenerComentarios(int idPublicacion)
    {
        List<Comentario> comentarios;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT c.Id, c.IdPublicacion, c.IdUsuarioComenta, c.Texto, c.FechaComentario,
                       u.NombreUsuario
                FROM Comentarios c
                INNER JOIN Usuarios u ON u.Id = c.IdUsuarioComenta
                WHERE c.IdPublicacion = @idPublicacion
                ORDER BY c.FechaComentario ASC";

            comentarios = connection.Query<Comentario, string, Comentario>(
                query,
                (comentario, nombreUsuario) =>
                {
                    comentario.NombreUsuario = nombreUsuario;
                    return comentario;
                },
                new { idPublicacion },
                splitOn: "NombreUsuario").ToList();
        }

        return comentarios;
    }

    // ---------------- Me Gusta ----------------

    public static bool FijarseSiExisteMeGusta(int idPublicacion, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT COUNT(*) FROM PublicacionesMeGusta
                             WHERE [IdPublicación] = @idPublicacion AND IdUsuario = @idUsuario";

            int count = connection.QueryFirstOrDefault<int>(query, new { idPublicacion, idUsuario });
            return count > 0;
        }
    }

    public static int ContarMeGusta(int idPublicacion)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"SELECT COUNT(*) FROM PublicacionesMeGusta WHERE [IdPublicación] = @idPublicacion";
            return connection.QueryFirstOrDefault<int>(query, new { idPublicacion });
        }
    }

    public static void AgregarMeGusta(int idPublicacion, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO PublicacionesMeGusta ([IdPublicación], IdUsuario)
                             VALUES (@idPublicacion, @idUsuario)";
            connection.Execute(query, new { idPublicacion, idUsuario });
        }
    }

    public static void QuitarMeGusta(int idPublicacion, int idUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"DELETE FROM PublicacionesMeGusta
                             WHERE [IdPublicación] = @idPublicacion AND IdUsuario = @idUsuario";
            connection.Execute(query, new { idPublicacion, idUsuario });
        }
    }

    // Toggle: si ya tenía me gusta lo saca, si no lo pone. Devuelve el nuevo estado.
    public static bool ToggleMeGusta(int idPublicacion, int idUsuario)
    {
        bool yaExiste = FijarseSiExisteMeGusta(idPublicacion, idUsuario);

        if (yaExiste == true)
        {
            QuitarMeGusta(idPublicacion, idUsuario);
            return false;
        }
        else
        {
            AgregarMeGusta(idPublicacion, idUsuario);
            return true;
        }
    }
}