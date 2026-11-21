import { Component, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ESTADOS_POSTULACION, EstadoPostulacion, Postulacion, cambiarEstado, claseEstadoPostulacion,
  cubiertasDe, postulaciones, transicionesEmpresa,
} from '../../../compartido/postulaciones/postulaciones';
import { datosEmpresa } from '../perfil-empresa/perfil-empresa';

@Component({
  selector: 'app-postulantes',
  imports: [DatePipe, FormsModule],
  templateUrl: './postulantes.html',
  styleUrl: '../../../compartido/postulaciones/postulaciones.css'
})
export class Postulantes {
  estados = ESTADOS_POSTULACION;
  clase = claseEstadoPostulacion;
  acciones = transicionesEmpresa;

  busqueda = signal('');
  filtroOferta = signal('');
  filtroEstado = signal('');
  error = signal('');

  // Con la API: las postulaciones a las ofertas de la empresa que inició sesión
  propias = computed(() => postulaciones().filter(p => p.empresa === datosEmpresa().nombreEmpresa));

  ofertas = computed(() => {
    const mapa = new Map<number, string>();
    this.propias().forEach(p => mapa.set(p.idOferta, p.puesto));
    return [...mapa].map(([id, puesto]) => ({ id, puesto }));
  });

  filtradas = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    return [...this.propias()]
      .filter(p =>
        (!texto || `${p.nombres} ${p.apellidos}`.toLowerCase().includes(texto)) &&
        (!this.filtroOferta() || String(p.idOferta) === this.filtroOferta()) &&
        (!this.filtroEstado() || p.estado === this.filtroEstado()))
      .sort((a, b) => b.fechaPostulacion.localeCompare(a.fechaPostulacion));
  });

  limpiarFiltros() {
    this.busqueda.set('');
    this.filtroOferta.set('');
    this.filtroEstado.set('');
  }

  // ---------- Detalle ----------
  seleccionadaId = signal<number | null>(null);
  seleccionada = computed(() => postulaciones().find(p => p.id === this.seleccionadaId()) ?? null);

  ver(p: Postulacion) {
    this.seleccionadaId.set(p.id);
    this.error.set('');
  }

  cerrar() { this.seleccionadaId.set(null); }

  // Vacantes que quedan en la oferta (se descuenta al marcar Seleccionado)
  disponibles(p: Postulacion): number {
    return p.vacantes - cubiertasDe(p.idOferta);
  }

  cambiar(p: Postulacion, estado: EstadoPostulacion) {
    this.error.set('');
    if (!cambiarEstado(p.id, estado)) {
      this.error.set('No se pudo cambiar el estado. Si intentabas seleccionar, la oferta ya no tiene vacantes disponibles.');
    }
  }

  // AAAA-MM -> MM/AAAA
  mes(valor: string): string {
    if (!valor) return '';
    const [a, m] = valor.split('-');
    return `${m}/${a}`;
  }
}