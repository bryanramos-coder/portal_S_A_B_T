import { Component, input, signal } from '@angular/core';

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
  foto = fotoPerfil;
}