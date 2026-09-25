function validarPublicacion() {
  const titulo = document.getElementById("titulo").value.trim();
  const descripcion = document.getElementById("descripcion").value.trim();
  const imagen = document.getElementById("imagen").value.trim();

  let esValido = true;
  limpiarFeedbacks();

  if (titulo === "") {
    mostrarError("fb-titulo", "Campo obligatorio");
    esValido = false;
  }

  if (descripcion === "") {
    mostrarError("fb-descripcion", "Campo obligatorio");
    esValido = false;
  }

  if (imagen === "") {
    mostrarError("fb-imagen", "Campo obligatorio");
    esValido = false;
  }

  // Si es false, el formulario no se envía
  if (esValido == true) {
    return true;
  } else {
    return false;
  }
}

function mostrarError(id, msg) {
  const el = document.getElementById(id);
  el.innerHTML = msg;
  el.style.color = "red";
}

function limpiarFeedbacks() {
  document.getElementById("fb-titulo").innerHTML = "";
  document.getElementById("fb-descripcion").innerHTML = "";
  document.getElementById("fb-imagen").innerHTML = "";
}
