const pasoTipo = document.getElementById('paso-tipo');
const pasoListo = document.getElementById('paso-listo');
const titulo = document.getElementById('titulo');
const titulos = {
    graduado: 'Registrar graduado o estudiante',
    empresa: 'Registrar empresa'
};
const formularios = {
    graduado: document.getElementById('form-graduado'),
    empresa: document.getElementById('form-empresa')
};

pasoTipo.querySelectorAll('[data-tipo]').forEach(function (boton) {
    boton.addEventListener('click', function () {
        pasoTipo.classList.add('form-oculto');
        formularios[boton.dataset.tipo].classList.remove('form-oculto');
        titulo.textContent = titulos[boton.dataset.tipo];

    });
});

Object.values(formularios).forEach(function (form) {
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        form.classList.add('form-oculto');
        pasoListo.classList.remove('form-oculto');
        titulo.textContent = 'Solicitud enviada';

    });
});

document.querySelectorAll('.ver-clave').forEach(function (btn) {
    btn.addEventListener('click', function () {
        const input = btn.previousElementSibling;
        const visible = input.type === 'text';
        input.type = visible ? 'password' : 'text';
        btn.textContent = visible ? 'Mostrar' : 'Ocultar';
        btn.setAttribute('aria-label', visible ? 'Mostrar contraseña' : 'Ocultar contraseña');
        btn.setAttribute('aria-pressed', String(!visible));
    });
});



document.querySelectorAll('.cambiar-tipo').forEach(function (enlace) {
    enlace.addEventListener('click', function (e) {
        e.preventDefault();
        Object.values(formularios).forEach(function (f) {
            f.classList.add('form-oculto');
        });
        pasoTipo.classList.remove('form-oculto');
        titulo.textContent = 'Crear cuenta';

    });
});

const selectSituacion = document.getElementById('g-situacion');
    const selectAnio = document.getElementById('g-anio');
    const anioActual = new Date().getFullYear();
    
    for (let i = anioActual; i >= 1990; i--) {
        let option = document.createElement('option');
        option.value = i;
        option.textContent = i;
        selectAnio.appendChild(option);
    }

selectSituacion.addEventListener('change', function() {
        const valor = this.value;

        if (valor === 'graduado' || valor === 'egresado') {
            // Si ya se graduó o egresó, habilitamos el selector de año
            selectAnio.disabled = false;
            selectAnio.required = true;
            selectAnio.options[0].text = "Seleccione su año";
        } else {
            // Si es estudiante activo (quinto año) o no ha seleccionado nada
            selectAnio.disabled = true;
            selectAnio.required = false;
            selectAnio.value = ""; // Limpia la selección previa si la había
            selectAnio.options[0].text = "No aplica (en curso)";
        }
    });