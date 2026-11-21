import { Component, computed, input, signal } from '@angular/core';
import { logoEmpresa } from '../../paginas/empresa/perfil-empresa/perfil-empresa';
import { fotoAlumno, datosAlumno } from '../../paginas/alumno/perfil-alumno/perfil-alumno';

export const FOTO_POR_DEFECTO = 'images/login.png';
const CLAVE = 'foto-perfil';

function leerFoto(): string {
  try {
    return localStorage.getItem(CLAVE) ?? FOTO_POR_DEFECTO;
  } catch {
    return FOTO_POR_DEFECTO;
  }
}

export const fotoPerfil = signal<string>(leerFoto());

export function guardarFoto(url: string) {
  fotoPerfil.set(url);
  try { localStorage.setItem(CLAVE, url); } catch {}
}

export function quitarFoto() {
  fotoPerfil.set(FOTO_POR_DEFECTO);
  try { localStorage.removeItem(CLAVE); } catch {}
}

@Component({
  selector: 'app-barra-superior',
  templateUrl: './barra-superior.html',
  styleUrl: './barra-superior.css'
})
export class BarraSuperior {
  nombre = input('APELLIDOS, NOMBRE');
  rol = input.required<string>();

  esEmpresa = computed(() => this.rol().toLowerCase() === 'empresa');
  esAlumno = computed(() => this.rol().toLowerCase() === 'alumno');

  // Empresa: su logo. Alumno: la foto de su perfil. Los demás roles: foto compartida.
  imagen = computed(() => {
    if (this.esEmpresa()) return logoEmpresa() || FOTO_POR_DEFECTO;
    if (this.esAlumno()) return fotoAlumno() || FOTO_POR_DEFECTO;
    return fotoPerfil();
  });

  nombreMostrado = computed(() => {
    if (this.esAlumno()) {
      const d = datosAlumno();
      const apellidos = d.apellidos.trim();
      const nombres = d.nombres.trim();
      if (apellidos || nombres) {
        return [apellidos, nombres].filter(Boolean).join(', ').toUpperCase();
      }
    }
    return this.nombre();
  });
}