import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../header/header';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, Header],
  templateUrl: './login.html',
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
