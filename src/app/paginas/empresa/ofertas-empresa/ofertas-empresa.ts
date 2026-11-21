import { Component, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  DEPARTAMENTOS, ESTADOS_OFERTA, FormOferta, MODALIDADES, NIVELES, Oferta, SECTORES, TIPOS, VEHICULOS,
  agregarHabilidad, claseEstadoOferta, datosDeOferta, formVacio, hoyLocal, limpiarOferta,
  lineas, rangoSalario, validarOferta,
} from '../../../compartido/ofertas/ofertas-comun';
import { cubiertasDe, postulantesDe } from '../../../compartido/postulaciones/postulaciones';
import {
  datosEmpresa, faltantesEmpresa, perfilEmpresaCompleto, CORREO_CUENTA
} from '../perfil-empresa/perfil-empresa';

function nombreContacto(): string {
  const p = datosEmpresa();
  return `${p.contactoNombres} ${p.contactoApellidos}`.trim();
}

function formNuevo(): FormOferta {
  return {
    ...formVacio(),
    empresa: datosEmpresa().nombreEmpresa,
    contactoNombre: nombreContacto(),
    contactoCorreo: CORREO_CUENTA,
  };
}

@Component({
  selector: 'app-ofertas-empresa',
  imports: [FormsModule, DatePipe, RouterLink],
  templateUrl: './ofertas-empresa.html',
  styleUrl: './ofertas-empresa.css'
})
export class OfertasEmpresa {
  departamentos = DEPARTAMENTOS;
  sectores = SECTORES;
  tipos = TIPOS;
  niveles = NIVELES;
  vehiculos = VEHICULOS;
  modalidades = MODALIDADES;
  estados = ESTADOS_OFERTA;

  clase = claseEstadoOferta;
  salario = rangoSalario;
  lineas = lineas;

  hoy = hoyLocal();

  // Estado del perfil: si está incompleto, no se puede publicar
  completo = perfilEmpresaCompleto;
  faltantes = faltantesEmpresa;
  empresa = datosEmpresa;

  // Se llenará con las ofertas de esta empresa que devuelva la API
  ofertas = signal<Oferta[]>([]);

  busqueda = signal('');
  filtroEstado = signal('');

  filtradas = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    return this.ofertas().filter(o =>
      (!texto ||
        o.puesto.toLowerCase().includes(texto) ||
        o.habilidades.some(h => h.toLowerCase().includes(texto))) &&
      (!this.filtroEstado() || o.estado === this.filtroEstado())
    );
  });

  limpiarFiltros() {
    this.busqueda.set('');
    this.filtroEstado.set('');
  }

  // Mientras no hay API, los seleccionados y postulantes salen de las postulaciones guardadas
  cubiertas(o: Oferta) { return Math.max(o.cubiertas, cubiertasDe(o.id)); }
  postulantes(o: Oferta) { return postulantesDe(o.id); }

  puedeEditar(o: Oferta) { return o.estado === 'Borrador' || o.estado === 'Publicada'; }
  puedePublicar(o: Oferta) { return o.estado === 'Borrador' && o.habilidades.length > 0 && this.completo(); }
  puedeCerrar(o: Oferta) { return o.estado === 'Publicada' || o.estado === 'Cubierta'; }

  // ---------- Detalle ----------
  seleccionada = signal<Oferta | null>(null);
  confirmandoCierre = signal(false);

  ver(oferta: Oferta) {
    this.seleccionada.set(oferta);
    this.confirmandoCierre.set(false);
  }

  cerrar() { this.seleccionada.set(null); }

  publicarDesdeDetalle() {
    const oferta = this.seleccionada();
    if (!oferta || !this.puedePublicar(oferta)) return;
    this.publicar(oferta);
    this.cerrar();
  }

  cerrarOfertaDesdeDetalle() {
    const oferta = this.seleccionada();
    if (!oferta || !this.puedeCerrar(oferta)) return;
    this.cerrarOferta(oferta);
    this.cerrar();
  }

  // ---------- Formulario ----------
  formAbierto = signal(false);
  editandoId: number | null = null;
  editandoEstado: string | null = null;
  form: FormOferta = formNuevo();
  nuevaHabilidad = '';
  errores = signal<Record<string, string>>({});
  usarPerfil = true;

  cambiarUsarPerfil(valor: boolean) {
    this.usarPerfil = valor;
    if (valor) {
      this.form.contactoNombre = nombreContacto();
      this.form.contactoCorreo = CORREO_CUENTA;
    }
  }

  nueva() {
    if (!this.completo()) return; // el perfil debe estar completo
    this.editandoId = null;
    this.editandoEstado = null;
    this.form = formNuevo();
    this.usarPerfil = true;
    this.nuevaHabilidad = '';
    this.errores.set({});
    this.formAbierto.set(true);
  }

  editar(oferta: Oferta) {
    if (!this.completo() || !this.puedeEditar(oferta)) return;
    this.cerrar();
    this.editandoId = oferta.id;
    this.editandoEstado = oferta.estado;
    this.form = datosDeOferta(oferta);
    this.usarPerfil =
      this.form.contactoNombre === nombreContacto() && this.form.contactoCorreo === CORREO_CUENTA;
    this.nuevaHabilidad = '';
    this.errores.set({});
    this.formAbierto.set(true);
  }

  cerrarFormulario() { this.formAbierto.set(false); }

  hayErrores() {
    return Object.keys(this.errores()).length > 0;
  }

  agregarHabilidad() {
    this.form.habilidades = agregarHabilidad(this.form.habilidades, this.nuevaHabilidad);
    this.nuevaHabilidad = '';
  }

  quitarHabilidad(i: number) {
    this.form.habilidades.splice(i, 1);
  }

  // publicar = true publica la oferta (exige al menos una habilidad).
  // Una oferta ya Publicada que se edita sigue publicada, así que también la exige.
  guardar(publicar: boolean) {
    if (!this.completo()) return; // protección extra
    const debePublicar = publicar || this.editandoEstado === 'Publicada';
    const e = validarOferta(this.form, this.hoy, { externa: false, publicar: debePublicar });
    this.errores.set(e);
    if (Object.keys(e).length > 0) return;

    const datos: FormOferta = { ...limpiarOferta(this.form), empresa: datosEmpresa().nombreEmpresa };
    this.guardarOferta(datos, this.editandoId, debePublicar);
    this.cerrarFormulario();
  }

  // Estas acciones llamarán a la API más adelante.
  // id_empresa e id_publicador salen de la sesión; la oferta nace en Borrador.
  guardarOferta(datos: FormOferta, id: number | null, publicar: boolean) {}
  publicar(oferta: Oferta) {}
  cerrarOferta(oferta: Oferta) {}
}