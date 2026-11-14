import { Injectable } from '@angular/core';

export type RespuestaLogin =
  | { estado: 'ok'; rol: 'graduado' | 'empresa' | 'coordinadora' }
  | { estado: 'pendiente' }
  | { estado: 'incorrecto' };

@Injectable({ providedIn: 'root' })
export class Auth {
  // TODO: reemplazar por HttpClient (provideHttpClient() en app.config.ts):
  //   return firstValueFrom(this.http.post<RespuestaLogin>('/api/login', { correo, clave }));
  async iniciarSesion(correo: string, clave: string): Promise<RespuestaLogin> {
    return { estado: 'incorrecto' };
  }

  async solicitarCuenta(tipo: 'graduado' | 'empresa', datos: unknown): Promise<void> {
    // TODO: POST al servidor con los datos del registro
  }
}
