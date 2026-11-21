import { Component, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  ESTADOS_POSTULACION, Postulacion, claseEstadoPostulacion, postulaciones, puedeRetirar, retirarPostulacion,
} from '../../../compartido/postulaciones/postulaciones';

@Component({
  selector: 'app-mis-postulaciones',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './mis-postulaciones.html',
  styleUrl: '../../../compartido/postulaciones/postulaciones.css'
})
export class MisPostulaciones {
  estados = ESTADOS_POSTULACION;
  clase = claseEstadoPostulacion;
  puedeRetirar = puedeRetirar;

  filtroEstado = signal('');
  confirmando = signal<number | null>(null);

  // Con la API: solo las postulaciones del alumno que inició sesión
  total = computed(() => postulaciones().length);

  lista = computed(() =>
    [...postulaciones()]
      .filter(p => !this.filtroEstado() || p.estado === this.filtroEstado())
      .sort((a, b) => b.fechaPostulacion.localeCompare(a.fechaPostulacion))
  );

  retirar(p: Postulacion) {
    retirarPostulacion(p.id);
    this.confirmando.set(null);
  }
}