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

  // La base de datos acepta años entre 1960 y 2100
  anios = Array.from({ length: new Date().getFullYear() - 1959 }, (_, i) => new Date().getFullYear() - i);

  // alumno: usuario.correo, datos_registro (nombres, apellidos, telefono), alumno (carnet, situación, año)
  alumno = this.fb.nonNullable.group({
    apellido: ['', [Validators.required, Validators.maxLength(100)]],
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
    correo: ['', [Validators.required, Validators.maxLength(150), Validators.pattern(CORREO)]],
    telefono: ['', [Validators.required, Validators.maxLength(20), Validators.pattern(TELEFONO)]],
    carnet: ['', [Validators.required, Validators.pattern(CARNET)]],
    situacion: ['', Validators.required],
    anio: [{ value: '', disabled: true }, Validators.required],
    clave: ['', [Validators.required, Validators.minLength(8)]],
  });

  // empresa: usuario.correo, datos_registro (persona de contacto), empresa (nombre_empresa, sitio_web),
  // solicitud_cuenta.declaracion_veracidad (terminos)
  empresa = this.fb.nonNullable.group({
    empresa: ['', [Validators.required, Validators.maxLength(150)]],
    correo: ['', [Validators.required, Validators.maxLength(150), Validators.pattern(CORREO)]],
    contactoNombres: ['', [Validators.required, Validators.maxLength(100)]],
    contactoApellidos: ['', [Validators.required, Validators.maxLength(100)]],
    telefono: ['', [Validators.required, Validators.maxLength(20), Validators.pattern(TELEFONO)]],
    web: ['', Validators.maxLength(200)],
    clave: ['', [Validators.required, Validators.pattern(CLAVE_EMPRESA)]],
    terminos: [false, Validators.requiredTrue],
  });

  anioLabel = signal('Año de egreso o de graduación');

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
        // Quinto año: anio_egreso_graduacion debe ser nulo
        this.anioLabel.set('Año de egreso o de graduación');
        anio.reset('');
        anio.disable();
      }
    });

    // El carnet se guarda en mayúsculas (CHECK de la base de datos)
    this.alumno.controls.carnet.valueChanges.subscribe(v => {
      const mayus = v.toUpperCase();
      if (v !== mayus) this.alumno.controls.carnet.setValue(mayus, { emitEvent: false });
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
    if (tipo === 'alumno') {
      await this.auth.solicitarCuenta('alumno', this.alumno.getRawValue());
    } else {
      await this.auth.solicitarCuenta('empresa', this.empresa.getRawValue());
    }
    this.paso.set('listo');
  }
}