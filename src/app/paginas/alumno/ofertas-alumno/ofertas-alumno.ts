import { Component, WritableSignal, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MODALIDADES, NIVELES, Oferta, SECTORES, TIPOS, hoyLocal, lineas, rangoSalario,
} from '../../../compartido/ofertas/ofertas-comun';
import {
  cubiertasDe, postulacionDe, postular, puedeRetirar, retirarPostulacion,
} from '../../../compartido/postulaciones/postulaciones';
import { faltantesAlumno, perfilAlumnoCompleto } from '../perfil-alumno/perfil-alumno';

// El logo vendrá de la API junto con cada oferta
type OfertaPublica = Oferta & { logo?: string };

const base = {
  ubicacionDetalle: '', vehiculo: '', salarioMin: null, salarioMax: null,
  habilidades: [] as string[], competencias: '', horarios: '', ofrecemos: '',
  vacantes: 1, cubiertas: 0, origen: 'Empresa' as const, estado: 'Publicada' as const,
  contactoNombre: 'Laura Gómez', contactoCorreo: 'rrhh@tecnosoft.com', indicaciones: '',
};

@Component({
  selector: 'app-ofertas-alumno',
  imports: [FormsModule, DatePipe, RouterLink],
  templateUrl: './ofertas-alumno.html',
  styleUrl: './ofertas-alumno.css'
})
export class OfertasAlumno {
  sectores = SECTORES;
  tipos = TIPOS;
  niveles = NIVELES;
  modalidades = MODALIDADES;
  opcionesPagina = [10, 20, 50];

  salario = rangoSalario;
  lineas = lineas;

  hoy = hoyLocal();

  // Si el perfil está incompleto, puede ver las ofertas pero no aplicar
  completo = perfilAlumnoCompleto;
  faltantes = faltantesAlumno;

  // Se llenará con las ofertas publicadas que devuelva la API
  ofertas = signal<OfertaPublica[]>([
    {
      ...base,
      id: 1, puesto: 'Desarrollador junior', empresa: 'Tecnosoft S.A.',
      sector: 'Tecnología | Informática', departamento: 'San Salvador',
      tipo: 'Tiempo completo', nivelExperiencia: 'Sin experiencia', modalidad: 'Híbrido',
      salarioMin: 600, salarioMax: 800, habilidades: ['Angular', 'C#', 'PostgreSQL'], vacantes: 2,
      objetivo: 'Participar en el desarrollo y mantenimiento de aplicaciones web.',
      funciones: 'Desarrollar funcionalidades en Angular.\nConsumir servicios de la API.',
      requisitos: 'Egresado o estudiante de Ingeniería de Sistemas.\nConocimientos básicos de TypeScript.',
      indicaciones: 'Menciona el puesto al escribir a la empresa.',
      fechaPublicacion: '2026-11-16', fechaExpiracion: '2026-12-30',
    },
    {
      ...base, id: 2, puesto: 'Técnico POS', empresa: 'Banco de América Central',
      sector: 'Bancos | Financieras', departamento: 'Santa Ana',
      tipo: 'Tiempo completo', nivelExperiencia: 'Menos de un año', modalidad: 'Presencial',
      vehiculo: 'Motocicleta', habilidades: ['Soporte técnico', 'Redes'],
      contactoNombre: 'Recursos Humanos', contactoCorreo: 'rrhh@banco.com',
      objetivo: 'Brindar soporte técnico y mantenimiento preventivo a terminales POS.',
      funciones: 'Visitar comercios asignados.\nRealizar mantenimiento preventivo.',
      requisitos: 'Bachiller técnico o estudiante de informática.\nMotocicleta propia y licencia vigente.',
      fechaPublicacion: '2026-11-06', fechaExpiracion: '2026-12-21',
    },
  ]);

  // Filtros
  textoBusqueda = '';
  busqueda = signal('');
  sector = signal('');
  tiposSel = signal<string[]>([]);
  nivelesSel = signal<string[]>([]);
  modalidadesSel = signal<string[]>([]);
  orden = signal<'recientes' | 'expiran' | 'puesto'>('recientes');
  porPagina = signal(10);
  pagina = signal(1);

  // Vacantes que aún quedan (puestos_vacantes - puestos_cubiertos).
  // Mientras no hay API, los seleccionados salen de las postulaciones guardadas.
  disponibles(o: OfertaPublica): number {
    return o.vacantes - Math.max(o.cubiertas, cubiertasDe(o.id));
  }

  // Solo se muestran ofertas Publicadas, sin expirar y con vacantes disponibles
  vigentes = computed(() =>
    this.ofertas().filter(o =>
      o.estado === 'Publicada' && o.fechaExpiracion >= this.hoy && this.disponibles(o) > 0)
  );

  conteoSectores = computed(() =>
    this.sectores
      .map(nombre => ({ nombre, total: this.vigentes().filter(o => o.sector === nombre).length }))
      .filter(a => a.total > 0)
  );

  filtradas = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    const lista = this.vigentes().filter(o =>
      (!texto ||
        o.puesto.toLowerCase().includes(texto) ||
        o.empresa.toLowerCase().includes(texto) ||
        o.habilidades.some(h => h.toLowerCase().includes(texto))) &&
      (!this.sector() || o.sector === this.sector()) &&
      (!this.tiposSel().length || this.tiposSel().includes(o.tipo)) &&
      (!this.nivelesSel().length || this.nivelesSel().includes(o.nivelExperiencia)) &&
      (!this.modalidadesSel().length || this.modalidadesSel().includes(o.modalidad))
    );
    const criterio = this.orden();
    return [...lista].sort((a, b) =>
      criterio === 'recientes' ? b.fechaPublicacion.localeCompare(a.fechaPublicacion)
      : criterio === 'expiran' ? a.fechaExpiracion.localeCompare(b.fechaExpiracion)
      : a.puesto.localeCompare(b.puesto)
    );
  });

  hayFiltros = computed(() =>
    !!(this.busqueda() || this.sector() || this.tiposSel().length || this.nivelesSel().length ||
       this.modalidadesSel().length)
  );

  totalPaginas = computed(() => Math.max(1, Math.ceil(this.filtradas().length / this.porPagina())));
  paginaActual = computed(() => Math.min(this.pagina(), this.totalPaginas()));
  paginas = computed(() => Array.from({ length: this.totalPaginas() }, (_, i) => i + 1));

  visibles = computed(() => {
    const inicio = (this.paginaActual() - 1) * this.porPagina();
    return this.filtradas().slice(inicio, inicio + this.porPagina());
  });

  // ---------- Acciones de filtros ----------
  buscar() {
    this.busqueda.set(this.textoBusqueda);
    this.pagina.set(1);
  }

  elegirSector(valor: string) {
    this.sector.set(valor);
    this.pagina.set(1);
  }

  alternar(lista: WritableSignal<string[]>, valor: string) {
    lista.update(l => (l.includes(valor) ? l.filter(x => x !== valor) : [...l, valor]));
    this.pagina.set(1);
  }

  cambiarOrden(valor: string) {
    this.orden.set(valor as 'recientes' | 'expiran' | 'puesto');
    this.pagina.set(1);
  }

  cambiarPorPagina(n: number) {
    this.porPagina.set(Number(n));
    this.pagina.set(1);
  }

  irAPagina(n: number) {
    this.pagina.set(Math.min(Math.max(1, n), this.totalPaginas()));
  }

  limpiar() {
    this.textoBusqueda = '';
    this.busqueda.set('');
    this.sector.set('');
    this.tiposSel.set([]);
    this.nivelesSel.set([]);
    this.modalidadesSel.set([]);
    this.pagina.set(1);
  }

  // ---------- Detalle ----------
  seleccionada = signal<OfertaPublica | null>(null);
  mensaje = signal('');
  copiado = signal(false);

  // Postulación del alumno a la oferta abierta (si existe)
  miPostulacion = computed(() => {
    const o = this.seleccionada();
    return o ? postulacionDe(o.id) : null;
  });

  ver(oferta: OfertaPublica) {
    this.seleccionada.set(oferta);
    this.mensaje.set('');
    window.scrollTo({ top: 0 });
  }

  cerrar() {
    this.seleccionada.set(null);
  }

  // Estado de la postulación del alumno a una oferta de la lista ('' si no se ha postulado)
  estadoPostulacion(o: OfertaPublica): string {
    const p = postulacionDe(o.id);
    return p && p.estado !== 'Retirada' ? p.estado : '';
  }

  cuantasDeEmpresa(o: OfertaPublica): number {
    return this.vigentes().filter(x => x.empresa === o.empresa).length;
  }

  verMasDeEmpresa(o: OfertaPublica) {
    this.limpiar();
    this.textoBusqueda = o.empresa;
    this.busqueda.set(o.empresa);
    this.cerrar();
  }

  verSector(o: OfertaPublica) {
    this.limpiar();
    this.sector.set(o.sector);
    this.cerrar();
  }

  // ---------- Postulación ----------
  // Aplicar y volver a aplicar usan la misma fila (UNIQUE id_oferta, id_alumno)
  aplicar(o: OfertaPublica) {
    if (!this.completo() || this.disponibles(o) <= 0) return;
    postular({ id: o.id, puesto: o.puesto, empresa: o.empresa, vacantes: o.vacantes });
    this.mensaje.set('Postulación enviada. Puedes seguirla en Mis postulaciones.');
  }

  retirarPostulacion(id: number) {
    retirarPostulacion(id);
    this.mensaje.set('Retiraste tu postulación. Puedes volver a aplicar cuando quieras.');
  }

  puedeRetirar = puedeRetirar;

  // ---------- Contacto opcional ----------
  tieneContacto(o: OfertaPublica): boolean {
    return !!(o.contactoNombre || o.contactoCorreo || o.indicaciones);
  }

  enlaceCorreo(o: OfertaPublica): string {
    if (!this.completo() || !o.contactoCorreo) return '';
    const asunto = encodeURIComponent('Interés en: ' + o.puesto);
    const cuerpo = encodeURIComponent(
      'Buenas tardes,\n\nMe interesa la plaza de ' + o.puesto +
      ' publicada en el Portal de Seguimiento y Bolsa de Trabajo.\n\n'
    );
    return `mailto:${o.contactoCorreo}?subject=${asunto}&body=${cuerpo}`;
  }

  async copiarCorreo(o: OfertaPublica) {
    if (!this.completo() || !o.contactoCorreo) return;
    try {
      await navigator.clipboard.writeText(o.contactoCorreo);
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2000);
    } catch {}
  }

  // ---------- Auxiliares ----------
  esNueva(o: OfertaPublica): boolean {
    const dias = (Date.now() - new Date(o.fechaPublicacion).getTime()) / (1000 * 60 * 60 * 24);
    return dias <= 7;
  }

  inicial(o: OfertaPublica): string {
    return (o.empresa || '?').trim().charAt(0).toUpperCase();
  }
}