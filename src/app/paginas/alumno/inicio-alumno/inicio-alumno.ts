import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  faltantesAlumno, perfilAlumnoCompleto, datosAlumno, situacionAlumno, totalObligatorios
} from '../perfil-alumno/perfil-alumno';

@Component({
  selector: 'app-inicio-alumno',
  imports: [RouterLink],
  templateUrl: './inicio-alumno.html',
  styleUrl: './inicio-alumno.css'
})
export class InicioAlumno {
  registro = datosAlumno;   // antes: registroAlumno (nombres, apellidos, etc.)
  tipo = situacionAlumno;   // antes: tipoAlumno (Quinto año, Egresado o Graduado)
  completo = perfilAlumnoCompleto;
  faltantes = faltantesAlumno;
  total = totalObligatorios;

  fecha = new Date().toLocaleDateString('es-SV', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
  });

  completados = computed(() => this.total() - this.faltantes().length);
  porcentaje = computed(() => Math.round((this.completados() / this.total()) * 100));
}