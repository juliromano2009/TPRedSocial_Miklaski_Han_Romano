
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tp_redSocial.Models;

namespace tp_redSocial.Controllers;

public class RedSocialController : Controller
{
    private const int CantidadPorPagina = 10;

    public IActionResult Index()
    {
        if (SesionValida())
        {
            return RedirectToAction(nameof(Inicio));
        }

        return View();
    }

    public IActionResult Registrarse()
    {
        return View();
    }

    public IActionResult Bienvenida()
    {
        if (SesionValida())
        {
            return View();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ValidarUsuario(string nombre, string apellido, string nombreUsuario, string contraseña)
    {
        Usuario usuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            NombreUsuario = nombreUsuario,
            Contraseña = contraseña
        };

        if (!Usuario.ValidarDatosRegistro(usuario.Nombre, usuario.Apellido, usuario.NombreUsuario, usuario.Contraseña))
        {
            return View("Registrarse");
        }

        if (Bd.FijarseSiExisteUsuario(nombreUsuario))
        {
            ViewBag.ErrorMessage = "El nombre de usuario ya existe. Por favor, elija otro.";
            return View("Registrarse");
        }

        Bd.AgregarUsuario(usuario);

        // Lo buscamos de nuevo para tener el Id que le asignó la base
        usuario = Bd.ObtenerUsuario(nombreUsuario, contraseña);

        HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Apellido", usuario.Apellido);

        return RedirectToAction(nameof(Inicio));
    }

    [HttpPost]
    public IActionResult IniciarSesion(string nombreUsuario, string contraseña)
    {
        Usuario usuario = Bd.ObtenerUsuario(nombreUsuario, contraseña);

        if (usuario != null)
        {
            HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            HttpContext.Session.SetString("Apellido", usuario.Apellido);
            return RedirectToAction(nameof(Inicio));
        }

        ViewBag.ErrorMessage = "Nombre de usuario o contraseña incorrectos.";
        return View("Index");
    }

    // ---------------- Publicaciones ----------------

    public IActionResult Inicio()
    {
        if (!SesionValida())
        {
            return RedirectToAction(nameof(Index));
        }

        int idUsuario = HttpContext.Session.GetInt32("IdUsuario").Value;
        List<Publicacion> publicaciones = Bd.ObtenerPublicaciones(0, CantidadPorPagina, idUsuario);

        // contamos cuantas publicaciones hay
        int cantidadPublicaciones = 0;
        foreach (Publicacion p in publicaciones)
        {
            cantidadPublicaciones = cantidadPublicaciones + 1;
        }

        ViewBag.Publicaciones = publicaciones;

        return View();
    }

    public IActionResult CrearPublicacion()
    {
        if (!SesionValida())
        {
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public IActionResult GuardarPublicacion(string titulo, string descripcion, string imagen)
    {
        if (!SesionValida())
        {
            return RedirectToAction(nameof(Index));
        }

        if (!Publicacion.ValidarDatosPublicacion(titulo, descripcion, imagen))
        {
            ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
            return View("CrearPublicacion");
        }

        Publicacion publicacion = new Publicacion
        {
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario").Value,
            Titulo = titulo,
            Descripcion = descripcion,
            Imagen = imagen,
            FechaPublicacion = DateTime.Now
        };

        Bd.AgregarPublicacion(publicacion);

        return RedirectToAction(nameof(Inicio));
    }

    // ---------------- Acciones que se llaman con Fetch ----------------

    [HttpPost]
    public IActionResult MeGusta(int idPublicacion)
    {
        if (!SesionValida())
        {
            return Json(new { ok = false, mensaje = "Tenés que iniciar sesión." });
        }

        if (Bd.ObtenerPublicacionPorId(idPublicacion) == null)
        {
            return Json(new { ok = false, mensaje = "La publicación no existe." });
        }

        int idUsuario = HttpContext.Session.GetInt32("IdUsuario").Value;

        bool leGusta = Bd.ToggleMeGusta(idPublicacion, idUsuario);
        int cantidad = Bd.ContarMeGusta(idPublicacion);

        return Json(new { ok = true, leGusta = leGusta, cantidad = cantidad });
    }

    [HttpPost]
    public IActionResult Comentar(int idPublicacion, string texto)
    {
        if (!SesionValida())
        {
            return Json(new { ok = false, mensaje = "Tenés que iniciar sesión." });
        }

        if (Bd.ObtenerPublicacionPorId(idPublicacion) == null)
        {
            return Json(new { ok = false, mensaje = "La publicación no existe." });
        }

        if (!Comentario.ValidarDatosComentario(texto))
        {
            return Json(new { ok = false, mensaje = "El comentario no puede estar vacío." });
        }

        Comentario comentario = new Comentario
        {
            IdPublicacion = idPublicacion,
            IdUsuarioComenta = HttpContext.Session.GetInt32("IdUsuario").Value,
            Texto = texto.Trim(),
            FechaComentario = DateTime.Now,
            NombreUsuario = HttpContext.Session.GetString("NombreUsuario")
        };

        comentario = Bd.AgregarComentario(comentario);

        return Json(new { ok = true, comentario = comentario });
    }

    [HttpGet]
    public IActionResult ObtenerMas(int desde)
    {
        if (!SesionValida())
        {
            return Json(new { ok = false, mensaje = "Tenés que iniciar sesión." });
        }

        int idUsuario = HttpContext.Session.GetInt32("IdUsuario").Value;
        List<Publicacion> publicaciones = Bd.ObtenerPublicaciones(desde, CantidadPorPagina, idUsuario);

        return Json(new { ok = true, publicaciones = publicaciones });
    }

    private bool SesionValida()
    {
        string nombreUsuario = HttpContext.Session.GetString("NombreUsuario");

        if (string.IsNullOrWhiteSpace(nombreUsuario) || HttpContext.Session.GetInt32("IdUsuario") == null)
        {
            return false;
        }

        bool existe = Bd.FijarseSiExisteUsuario(nombreUsuario);

        if (!existe)
        {
            HttpContext.Session.Clear();
        }

        return existe;
    }
}
