import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Header } from '../header/header';
import { Auth } from '../services/auth';

type Paso = 'tipo' | 'graduado' | 'empresa' | 'listo';

const CORREO = /^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/;
const TELEFONO = /^\+?(?=(?:[^0-9]*[0-9]){8,15}$)[0-9][0-9\s\-]*[0-9]$/;
const CARNET = /^[A-Z]{2}[0-9]{3,6}$/;
const CLAVE_EMPRESA = /^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$/;

@Component({
  selector: 'app-registro',
  imports: [ReactiveFormsModule, RouterLink, Header],
  template: `
    <app-header [enlaces]="[{ texto: 'Inicio', ruta: '/' }, { texto: 'Iniciar sesión', ruta: '/login' }]" />

    <main>
      <div class="login-card">
        <div class="logo-container">
          <img src="/images/registro.png" alt="Crear cuenta" class="login-img">
        </div>

        <div class="form-group"><h1>{{ titulo[paso()] }}</h1></div>

        @switch (paso()) {

          @case ('tipo') {
            <div class="form-group">
              <label class="QueTipo">¿Qué tipo de cuenta quieres crear?</label>
            </div>
            <div class="tipo-opciones">
              <button type="button" class="btn-submit" (click)="paso.set('graduado')">Graduado o estudiante</button>
              <button type="button" class="btn-submit" (click)="paso.set('empresa')">Empresa</button>
            </div>
          }

          @case ('graduado') {
            <form style="width: 100%;" [formGroup]="graduado" (ngSubmit)="enviar('graduado')">
              <label for="g-apellido">Apellidos</label>
              <div class="form-group"><input type="text" id="g-apellido" formControlName="apellido" placeholder="Ej: Arevalo Melara"></div>

              <label for="g-nombre">Nombres</label>
              <div class="form-group"><input type="text" id="g-nombre" formControlName="nombre" placeholder="Ej: Juan Natanael"></div>

              <label for="g-correo">Correo electrónico</label>
              <div class="form-group"><input type="email" id="g-correo" formControlName="correo" placeholder="Ej: estudiante@gmail.com"></div>

              <label for="g-telefono">Número de celular</label>
              <div class="form-group"><input type="tel" id="g-telefono" formControlName="telefono" placeholder="Ej: +503 2222-3333"></div>

              <label for="g-carnet">Carnet</label>
              <div class="form-group">
                <input type="text" id="g-carnet" formControlName="carnet" placeholder="Ej: RC23011"
                       title="El carné debe iniciar con dos letras mayúsculas seguidas de números (Ej: RC23011)">
              </div>

              <label for="g-situacion">Situación</label>
              <div class="form-group">
                <select id="g-situacion" formControlName="situacion">
                  <option value="">Selecciona una opción</option>
                  <option value="graduado">Graduado</option>
                  <option value="egresado">Egresado</option>
                  <option value="quinto">Estudiante de quinto año o en trámite de graduación</option>
                </select>
              </div>

              <label for="g-anio">Año de graduación o de egreso</label>
              <div class="form-group">
                <select id="g-anio" formControlName="anio">
                  <option value="">{{ graduado.controls.anio.disabled ? 'No aplica (en curso)' : 'Seleccione su año' }}</option>
                  @for (a of anios; track a) { <option [value]="a">{{ a }}</option> }
                </select>
              </div>

              <label for="g-clave">Contraseña</label>
              <div class="campo-clave">
                <input [type]="verClaveG() ? 'text' : 'password'" id="g-clave" formControlName="clave" placeholder="Mínimo 8 caracteres">
                <button type="button" class="ver-clave" [attr.aria-pressed]="verClaveG()"
                        [attr.aria-label]="verClaveG() ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                        (click)="verClaveG.set(!verClaveG())">{{ verClaveG() ? 'Ocultar' : 'Mostrar' }}</button>
              </div>

              <button type="submit" class="btn-submit">Solicitar cuenta</button>
              <div class="links"><a href="#" (click)="volver($event)">Cambiar tipo de cuenta</a></div>
            </form>
          }

          @case ('empresa') {
            <form style="width: 100%;" [formGroup]="empresa" (ngSubmit)="enviar('empresa')">
              <label for="e-empresa">Nombre de la empresa</label>
              <div class="form-group"><input type="text" id="e-empresa" formControlName="empresa" placeholder="Ej: Central American Software Services"></div>

              <label for="e-correo">Correo electrónico</label>
              <div class="form-group"><input type="email" id="e-correo" formControlName="correo" placeholder="Ej: empresa@gmail.com"></div>

              <label for="e-contacto">Persona de contacto</label>
              <div class="form-group"><input type="text" id="e-contacto" formControlName="contacto" placeholder="Ej: Ramon Alberto Muñoz Gonzales"></div>

              <label for="e-telefono">Teléfono de contacto</label>
              <div class="form-group"><input type="tel" id="e-telefono" formControlName="telefono" placeholder="Ej: +503 2222-3333"></div>

              <label for="e-web">Sitio web</label>
              <div class="form-group"><input type="url" id="e-web" formControlName="web" placeholder="https://empresa.com.sv"></div>

              <label for="e-clave">Contraseña</label>
              <div class="campo-clave">
                <input [type]="verClaveE() ? 'text' : 'password'" id="e-clave" formControlName="clave" placeholder="Mínimo 8 caracteres, letras y números">
                <button type="button" class="ver-clave" [attr.aria-pressed]="verClaveE()"
                        [attr.aria-label]="verClaveE() ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                        (click)="verClaveE.set(!verClaveE())">{{ verClaveE() ? 'Ocultar' : 'Mostrar' }}</button>
              </div>

              <div class="terminos">
                <input type="checkbox" id="e-terminos" formControlName="terminos">
                <label for="e-terminos">
                  Declaro que la información proporcionada es verídica y que represento a esta empresa.
                  Acepto los <a href="terminos.html" target="_blank" rel="noopener">términos y condiciones</a>.
                </label>
              </div>

              <button type="submit" class="btn-submit">Solicitar cuenta</button>
              <div class="links"><a href="#" (click)="volver($event)">Cambiar tipo de cuenta</a></div>
            </form>
          }

          @case ('listo') {
            <div class="aviso" role="status">
              <strong>Solicitud de registro enviada.</strong>
              Su cuenta queda pendiente hasta que la administración la apruebe.
              Cuando esté lista podrá
              <div class="links"><a routerLink="/login">iniciar sesión</a></div>
            </div>
          }
        }
      </div>
    </main>
  `,
})
export class Registro {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);

  paso = signal<Paso>('tipo');
  verClaveG = signal(false);
  verClaveE = signal(false);

  titulo: Record<Paso, string> = {
    tipo: 'Crear cuenta',
    graduado: 'Registrar graduado o estudiante',
    empresa: 'Registrar empresa',
    listo: 'Solicitud enviada',
  };

  anios = Array.from({ length: new Date().getFullYear() - 1989 }, (_, i) => new Date().getFullYear() - i);

  graduado = this.fb.nonNullable.group({
    apellido: ['', Validators.required],
    nombre: ['', Validators.required],
    correo: ['', [Validators.required, Validators.pattern(CORREO)]],
    telefono: ['', [Validators.required, Validators.pattern(TELEFONO)]],
    carnet: ['', [Validators.required, Validators.pattern(CARNET)]],
    situacion: ['', Validators.required],
    anio: [{ value: '', disabled: true }, Validators.required],
    clave: ['', [Validators.required, Validators.minLength(8)]],
  });

  empresa = this.fb.nonNullable.group({
    empresa: ['', Validators.required],
    correo: ['', [Validators.required, Validators.pattern(CORREO)]],
    contacto: ['', Validators.required],
    telefono: ['', [Validators.required, Validators.pattern(TELEFONO)]],
    web: [''],
    clave: ['', [Validators.required, Validators.pattern(CLAVE_EMPRESA)]],
    terminos: [false, Validators.requiredTrue],
  });

  constructor() {
    // El año solo aplica a graduados y egresados
    this.graduado.controls.situacion.valueChanges.subscribe(valor => {
      const anio = this.graduado.controls.anio;
      if (valor === 'graduado' || valor === 'egresado') {
        anio.enable();
      } else {
        anio.reset('');
        anio.disable();
      }
    });
  }

  volver(e: Event) {
    e.preventDefault();
    this.paso.set('tipo');
  }

  async enviar(tipo: 'graduado' | 'empresa') {
    const form = this[tipo];
    if (form.invalid) {
      form.markAllAsTouched();
      return;
    }
    await this.auth.solicitarCuenta(tipo, form.getRawValue());
    this.paso.set('listo');
  }
}
