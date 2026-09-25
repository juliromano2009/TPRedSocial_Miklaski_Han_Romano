// Cuántas publicaciones ya se muestran (al entrar se muestran las primeras 10)
let desde = 10;


function darMeGusta(idPublicacion) {
  const datos = new FormData();
  datos.append("idPublicacion", idPublicacion);

  fetch("/RedSocial/MeGusta", { method: "POST", body: datos })
    .then(response => response.json())
    .then(data => {
      if (!data.ok) {
        alert(data.mensaje);
        return;
      }

      document.getElementById("cantidad-megusta-" + idPublicacion).innerHTML = data.cantidad;

      if (data.leGusta) {
        document.getElementById("btn-megusta-" + idPublicacion).innerHTML = "Ya no me gusta";
      } else {
        document.getElementById("btn-megusta-" + idPublicacion).innerHTML = "Me gusta";
      }
    })
}

// ---------------- Comentarios ----------------

function comentar(idPublicacion) {
  const inputTexto = document.getElementById("texto-comentario-" + idPublicacion);
  const texto = inputTexto.value.trim();
  const idFeedback = "fb-comentario-" + idPublicacion;

  if (texto === "") {
    mostrarError(idFeedback, "El comentario no puede estar vacío.");
    return;
  }

  const datos = new FormData();
  datos.append("idPublicacion", idPublicacion);
  datos.append("texto", texto);

  fetch("/RedSocial/Comentar", { method: "POST", body: datos })
    .then(response => response.json())
    .then(data => {
      if (!data.ok) {
        mostrarError(idFeedback, data.mensaje);
        return;
      }

      // Agregamos el comentario nuevo abajo de los que ya estaban
      // (insertAdjacentHTML agrega al final sin volver a armar todo el div)
      document.getElementById("comentarios-" + idPublicacion).insertAdjacentHTML("beforeend", crearComentarioHtml(data.comentario));
      inputTexto.value = "";
      document.getElementById(idFeedback).innerHTML = "";
    })
    .catch(error => {
      console.log(error);
      mostrarError(idFeedback, "Hubo un error al comentar.");
    });
}

// ---------------- Ver más ----------------

function verMas() {
  fetch("/RedSocial/ObtenerMas?desde=" + desde)
    .then(response => response.json())
    .then(data => {
      if (!data.ok) {
        mostrarError("fb-ver-mas", data.mensaje);
        return;
      }

      const divPublicaciones = document.getElementById("publicaciones");

      for (const publicacion of data.publicaciones) {
        divPublicaciones.insertAdjacentHTML("beforeend", crearPublicacionHtml(publicacion));
      }

      desde = desde + data.publicaciones.length;

      // Si vinieron menos de 10, ya no quedan más publicaciones
      if (data.publicaciones.length < 10) {
        document.getElementById("btn-ver-mas").style.display = "none";
        mostrarOk("fb-ver-mas", "No hay más publicaciones.");
      }
    })
    .catch(error => {
      console.log(error);
      mostrarError("fb-ver-mas", "Hubo un error al cargar más publicaciones.");
    });
}

function crearPublicacionHtml(publicacion) {
  let comentariosHtml = "";
  for (const comentario of publicacion.comentarios) {
    comentariosHtml += crearComentarioHtml(comentario);
  }

  let textoBoton = "Me gusta";
  if (publicacion.usuarioActualDioMeGusta) {
    textoBoton = "Ya no me gusta";
  }

  return [
    `<div class="publicacion" id="publicacion-${publicacion.id}">`,
    `<img src="/img/${escaparHtml(publicacion.imagen)}" alt="${escaparHtml(publicacion.titulo)}" width="300">`,
    `<h3>${escaparHtml(publicacion.titulo)}</h3>`,
    `<p>${escaparHtml(publicacion.descripcion)}</p>`,
    `<p>Publicado por <strong>${escaparHtml(publicacion.nombreUsuario)}</strong> el ${formatearFecha(publicacion.fechaPublicacion)}</p>`,
    `<button id="btn-megusta-${publicacion.id}" onclick="darMeGusta(${publicacion.id})">${textoBoton}</button>`,
    `<span id="cantidad-megusta-${publicacion.id}">${publicacion.cantidadMeGusta}</span> Me gusta`,
    `<h4>Comentarios</h4>`,
    `<div id="comentarios-${publicacion.id}">${comentariosHtml}</div>`,
    `<input type="text" id="texto-comentario-${publicacion.id}" placeholder="Escribí un comentario">`,
    `<button onclick="comentar(${publicacion.id})">Comentar</button>`,
    `<p id="fb-comentario-${publicacion.id}" class="feedback"></p>`,
    `<hr></div>`
  ].join("");
}

function crearComentarioHtml(comentario) {
  return `<p><strong>${escaparHtml(comentario.nombreUsuario)}</strong> (${formatearFecha(comentario.fechaComentario)}): ${escaparHtml(comentario.texto)}</p>`;
}

// ---------------- Funciones de ayuda ----------------

// Pasa la fecha que manda C# al formato dd/MM/yyyy HH:mm (igual que en la vista)
function formatearFecha(fechaTexto) {
  const fecha = new Date(fechaTexto);
  const dia = String(fecha.getDate()).padStart(2, "0");
  const mes = String(fecha.getMonth() + 1).padStart(2, "0");

  return dia + "/" + mes;
}

// Evita que si alguien escribe HTML en un comentario (ej: <script>) se ejecute en la página
function escaparHtml(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML.replaceAll('"', "&quot;");
}

function mostrarError(id, msg) {
  const el = document.getElementById(id);
  el.innerHTML = msg;
  el.style.color = "red";
}

function mostrarOk(id, mensaje) {
  const el = document.getElementById(id);
  el.innerHTML = mensaje;
  el.style.color = "green";
}
