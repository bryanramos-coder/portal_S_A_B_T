import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { postulaciones } from '../../../compartido/postulaciones/postulaciones';

@Component({
  selector: 'app-inicio-admin',
  imports: [RouterLink],
  templateUrl: './inicio-admin.html',
  styleUrl: './inicio-admin.css'
})
export class InicioAdmin {

  fecha = new Date().toLocaleDateString('es-SV', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
  });

  indicadores = [
    { titulo: 'Egresados registrados', valor: 126, detalle: '+8 este mes',     color: '#3b82f6' },
    { titulo: 'Empresas activas',      valor: 34,  detalle: '+2 este mes',     color: '#22c55e' },
    { titulo: 'Ofertas publicadas',    valor: 58,  detalle: '12 por vencer',   color: '#f59e0b' },
    { titulo: 'Alumnos registrados',   valor: 210, detalle: '+15 este mes',    color: '#a855f7' },
  ];

  // Mientras no hay API salen de las postulaciones guardadas en el navegador
  totalPostulaciones = computed(() => postulaciones().length);
  enCurso = computed(() =>
    postulaciones().filter(p => p.estado === 'Recibida' || p.estado === 'En revisión').length
  );
  seleccionados = computed(() =>
    postulaciones().filter(p => p.estado === 'Seleccionado').length
  );
}