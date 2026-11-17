import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Header } from '../header/header';
import { Auth } from '../services/auth';

type Paso = 'tipo' | 'alumno' | 'empresa' | 'listo';

const CORREO = /^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/;
const TELEFONO = /^\+?(?=(?:[^0-9]*[0-9]){8,15}$)[0-9][0-9\s\-]*[0-9]$/;
const CARNET = /^[A-Z]{2}[0-9]{3,6}$/;
const CLAVE_EMPRESA = /^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$/;

@Component({
  selector: 'app-registro',
  imports: [ReactiveFormsModule, RouterLink, Header],
  templateUrl: './registro.html',
})
export class Registro {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);

  paso = signal<Paso>('tipo');
  verClaveG = signal(false);
  verClaveE = signal(false);

  titulo: Record<Paso, string> = {
    tipo: 'Crear cuenta',
    alumno: 'Registrar graduado o estudiante',
    empresa: 'Registrar empresa',
    listo: 'Solicitud enviada',
  };

  anios = Array.from({ length: new Date().getFullYear() - 1989 }, (_, i) => new Date().getFullYear() - i);

  alumno = this.fb.nonNullable.group({
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

 anioLabel = signal('Año de graduación o de egreso');

  constructor() {
    this.alumno.controls.situacion.valueChanges.subscribe(valor => {
      const anio = this.alumno.controls.anio;
      if (valor === 'graduado') {
        this.anioLabel.set('Año de graduación');
        anio.enable();
      } else if (valor === 'egresado') {
        this.anioLabel.set('Año de egreso');
        anio.enable();
      } else {
        this.anioLabel.set('Año estimado de graduación'); // Para quinto año
        anio.reset('');
        anio.disable();
      }
    });
  }

  volver(e: Event) {
    e.preventDefault();
    this.paso.set('tipo');
  }

  async enviar(tipo: 'alumno' | 'empresa') {
    const form = this[tipo];
    if (form.invalid) {
      form.markAllAsTouched();
      return;
    }
    await this.auth.solicitarCuenta(tipo, form.getRawValue());
    this.paso.set('listo');
  }
}
