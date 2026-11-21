import { Component, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  DEPARTAMENTOS, ESTADOS_OFERTA, FormOferta, MODALIDADES, NIVELES, Oferta, SECTORES, TIPOS, VEHICULOS,
  agregarHabilidad, claseEstadoOferta, datosDeOferta, formVacio, hoyLocal, limpiarOferta,
  lineas, rangoSalario, validarOferta,
} from '../../../compartido/ofertas/ofertas-comun';

// Otras pantallas importan estos tipos desde aquí
export type { Oferta, FormOferta, EstadoOferta } from '../../../compartido/ofertas/ofertas-comun';

@Component({
  selector: 'app-ofertas-admin',
  imports: [FormsModule, DatePipe],
  templateUrl: './ofertas-admin.html',
  styleUrl: './ofertas-admin.css'
})
export class OfertasAdmin {
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

  // Se llenará con los datos que devuelva la API
  ofertas = signal<Oferta[]>([]);

  busqueda = signal('');
  filtroEstado = signal('');
  filtroTipo = signal('');

  filtradas = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    return this.ofertas().filter(o =>
      (!texto ||
        o.puesto.toLowerCase().includes(texto) ||
        o.empresa.toLowerCase().includes(texto) ||
        o.habilidades.some(h => h.toLowerCase().includes(texto))) &&
      (!this.filtroEstado() || o.estado === this.filtroEstado()) &&
      (!this.filtroTipo() || o.tipo === this.filtroTipo())
    );
  });

  limpiarFiltros() {
    this.busqueda.set('');
    this.filtroEstado.set('');
    this.filtroTipo.set('');
  }

  // El administrador solo gestiona las ofertas que él cargó (empresas externas)
  esPropia(o: Oferta) { return o.origen === 'Administrador'; }
  puedeEditar(o: Oferta) { return this.esPropia(o) && (o.estado === 'Borrador' || o.estado === 'Publicada'); }
  puedePublicar(o: Oferta) { return this.esPropia(o) && o.estado === 'Borrador' && o.habilidades.length > 0; }
  puedeCerrar(o: Oferta) { return this.esPropia(o) && (o.estado === 'Publicada' || o.estado === 'Cubierta'); }

  // ---------- Ventana de detalle ----------
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

  // ---------- Formulario de crear o editar ----------
  formAbierto = signal(false);
  editandoId: number | null = null;
  editandoEstado: string | null = null;
  form: FormOferta = formVacio();
  nuevaHabilidad = '';
  errores = signal<Record<string, string>>({});

  nueva() {
    this.editandoId = null;
    this.editandoEstado = null;
    this.form = formVacio();
    this.nuevaHabilidad = '';
    this.errores.set({});
    this.formAbierto.set(true);
  }

  editar(oferta: Oferta) {
    if (!this.puedeEditar(oferta)) return;
    this.cerrar();
    this.editandoId = oferta.id;
    this.editandoEstado = oferta.estado;
    this.form = datosDeOferta(oferta);
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

  // publicar = true crea la oferta ya publicada (o la publica si era borrador).
  // Una oferta Publicada que se edita sigue publicada, así que también exige habilidades.
  guardar(publicar: boolean) {
    const debePublicar = publicar || this.editandoEstado === 'Publicada';
    const e = validarOferta(this.form, this.hoy, { externa: true, publicar: debePublicar });
    this.errores.set(e);
    if (Object.keys(e).length > 0) return;
    this.guardarOferta(limpiarOferta(this.form), this.editandoId, debePublicar);
    this.cerrarFormulario();
  }

  // Estas acciones llamarán a la API más adelante.
  // empresa_externa = form.empresa e id_publicador = el administrador.
  guardarOferta(datos: FormOferta, id: number | null, publicar: boolean) {}
  publicar(oferta: Oferta) {}
  cerrarOferta(oferta: Oferta) {}
}