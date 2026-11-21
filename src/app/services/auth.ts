import { Injectable } from '@angular/core';

export type RespuestaLogin =
  | { estado: 'ok'; rol: 'alumno' | 'empresa' | 'admin' }
  | { estado: 'pendiente' }
  | { estado: 'suspendida' }
  | { estado: 'rechazada' }
  | { estado: 'incorrecto' };

// Datos que envía el formulario de registro del alumno
export interface SolicitudAlumno {
  apellido: string;
  nombre: string;
  correo: string;
  telefono: string;
  carnet: string;
  situacion: string;   // 'graduado' | 'egresado' | 'quinto'
  anio: string;        // vacío si es de quinto año
  clave: string;
}

// Datos que envía el formulario de registro de la empresa
export interface SolicitudEmpresa {
  empresa: string;
  correo: string;
  contactoNombres: string;
  contactoApellidos: string;
  telefono: string;
  web: string;
  clave: string;
  terminos: boolean;   // solicitud_cuenta.declaracion_veracidad
}

@Injectable({ providedIn: 'root' })
export class Auth {
  // TODO: reemplazar por HttpClient (provideHttpClient() en app.config.ts):
  //   return firstValueFrom(this.http.post<RespuestaLogin>('/api/login', { correo, clave }));
  async iniciarSesion(correo: string, clave: string): Promise<RespuestaLogin> {
    return { estado: 'incorrecto' };
  }

  async solicitarCuenta(tipo: 'alumno', datos: SolicitudAlumno): Promise<void>;
  async solicitarCuenta(tipo: 'empresa', datos: SolicitudEmpresa): Promise<void>;
  async solicitarCuenta(tipo: 'alumno' | 'empresa', datos: SolicitudAlumno | SolicitudEmpresa): Promise<void> {
    // TODO: POST al servidor con los datos del registro
  }
}