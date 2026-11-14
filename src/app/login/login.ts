import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../header/header';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, Header],
  template: `
    <app-header [enlaces]="[{ texto: 'Inicio', ruta: '/' }, { texto: 'Crear cuenta', ruta: '/registro' }]" />

    <main>
      <div class="login-card">
        <div class="logo-container">
          <img src="/images/login.png" alt="Iniciar sesión" class="login-img">
        </div>

        <h1>Iniciar sesión</h1>

        @if (mensaje()) {
          <div class="aviso" role="alert">{{ mensaje() }}</div>
        }

        <form style="width: 100%;" [formGroup]="form" (ngSubmit)="enviar()">
          <label for="username">Email</label>
          <div class="form-group">
            <input type="email" id="username" formControlName="correo" placeholder="Ej: correo@gmail.com">
          </div>

          <label for="claveP">Contraseña</label>
          <div class="campo-clave">
            <input [type]="verClave() ? 'text' : 'password'" id="claveP" formControlName="clave"
                   autocomplete="current-password" placeholder="Mínimo 8 caracteres">
            <button type="button" class="ver-clave" [attr.aria-pressed]="verClave()"
                    [attr.aria-label]="verClave() ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                    (click)="verClave.set(!verClave())">{{ verClave() ? 'Ocultar' : 'Mostrar' }}</button>
          </div>

          <div class="links">
            <a href="#">¿Has olvidado tu contraseña?</a>
          </div>

          <button type="submit" class="btn-submit">Iniciar sesión</button>
        </form>
      </div>
    </main>
  `,
})
export class Login {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);
  private router = inject(Router);

  // Pantalla de cada rol (cambia por tus rutas reales)
  private readonly pantallaPorRol = {
    graduado: '/graduado',
    empresa: '/empresa',
    coordinadora: '/coordinadora',
  };

  verClave = signal(false);
  mensaje = signal('');

  form = this.fb.nonNullable.group({
    correo: ['', [Validators.required, Validators.pattern(/^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/)]],
    clave: ['', Validators.required],
  });

  async enviar() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.mensaje.set('Escribe un correo válido y tu contraseña.');
      return;
    }
    const { correo, clave } = this.form.getRawValue();
    const r = await this.auth.iniciarSesion(correo, clave);

    if (r.estado === 'ok') {
      this.router.navigateByUrl(this.pantallaPorRol[r.rol]);
    } else if (r.estado === 'pendiente') {
      this.mensaje.set('Tu solicitud sigue pendiente de aprobación. Te avisaremos cuando esté lista.');
    } else {
      this.mensaje.set('El correo o la contraseña no coinciden. Revisa tus datos e inténtalo de nuevo.');
    }
  }
}
