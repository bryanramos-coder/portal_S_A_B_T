import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

// empresa: nombre_empresa, nit, descripcion, sitio_web
// datos_registro: nombres, apellidos, telefono, cargo (el logo va en foto_url)
export interface DatosEmpresa {
  nombreEmpresa: string;     // empresa.nombre_empresa VARCHAR(150)
  nit: string;               // empresa.nit VARCHAR(20)
  descripcion: string;       // empresa.descripcion TEXT
  sitioWeb: string;          // empresa.sitio_web VARCHAR(200)
  contactoNombres: string;   // datos_registro.nombres VARCHAR(100)
  contactoApellidos: string; // datos_registro.apellidos VARCHAR(100)
  telefono: string;          // datos_registro.telefono VARCHAR(20)
  cargoContacto: string;     // datos_registro.cargo VARCHAR(100), opcional
}

// usuario.correo. Con la API vendrá de la sesión.
export const CORREO_CUENTA = 'empresa@correo.com';

const CLAVE = 'perfil-empresa';
const CLAVE_LOGO = 'logo-empresa';
const TELEFONO = /^\+?(?=(?:[^0-9]*[0-9]){8,15}$)[0-9][0-9\s\-]*[0-9]$/;

function vacio(): DatosEmpresa {
  return {
    nombreEmpresa: '', nit: '', descripcion: '', sitioWeb: '',
    contactoNombres: '', contactoApellidos: '', telefono: '', cargoContacto: '',
  };
}

// Datos que la empresa escribió en el registro. Con la API vendrán del servidor.
function datosDeRegistro(): Partial<DatosEmpresa> {
  return {
    nombreEmpresa: 'Tecnosoft S.A.',
    sitioWeb: 'https://www.tecnosoft.com',
    contactoNombres: 'Laura',
    contactoApellidos: 'Gómez',
    telefono: '+503 7123-4567',
  };
}

function leer(): DatosEmpresa {
  const base: DatosEmpresa = { ...vacio(), ...datosDeRegistro() };
  try {
    const guardado = localStorage.getItem(CLAVE);
    if (!guardado) return base;
    const g = JSON.parse(guardado);
    // Solo se toman los campos actuales (descarta lo que guardó la versión anterior)
    (Object.keys(base) as (keyof DatosEmpresa)[]).forEach(k => {
      if (typeof g[k] === 'string') base[k] = g[k];
    });
    return base;
  } catch {
    return base;
  }
}

function leerLogo(): string {
  try {
    return localStorage.getItem(CLAVE_LOGO) ?? '';
  } catch {
    return '';
  }
}

// Campos que la empresa debe llenar antes de poder publicar.
// La base de datos solo exige los del registro; el NIT y la descripción los pide esta pantalla.
export const OBLIGATORIOS: { campo: keyof DatosEmpresa; etiqueta: string }[] = [
  { campo: 'nombreEmpresa', etiqueta: 'nombre de la empresa' },
  { campo: 'nit', etiqueta: 'NIT' },
  { campo: 'descripcion', etiqueta: 'descripción' },
  { campo: 'contactoNombres', etiqueta: 'nombres del contacto' },
  { campo: 'contactoApellidos', etiqueta: 'apellidos del contacto' },
  { campo: 'telefono', etiqueta: 'teléfono' },
];

export const datosEmpresa = signal<DatosEmpresa>(leer());
// datos_registro.foto_url. Con la API será la URL de la imagen subida al servidor.
export const logoEmpresa = signal<string>(leerLogo());

export const faltantesEmpresa = computed(() =>
  OBLIGATORIOS
    .filter(o => !String(datosEmpresa()[o.campo] ?? '').trim())
    .map(o => o.etiqueta)
);

export const perfilEmpresaCompleto = computed(() => faltantesEmpresa().length === 0);

@Component({
  selector: 'app-perfil-empresa',
  imports: [FormsModule],
  templateUrl: './perfil-empresa.html',
  styleUrl: './perfil-empresa.css'
})
export class PerfilEmpresa {
  correo = CORREO_CUENTA;

  form: DatosEmpresa = { ...datosEmpresa() };
  errores = signal<Record<string, string>>({});
  mensaje = signal('');

  faltantes = faltantesEmpresa;
  completo = perfilEmpresaCompleto;
  total = OBLIGATORIOS.length;

  // Logo (opcional)
  logo = logoEmpresa;
  errorLogo = signal('');

  get completados() {
    return this.total - faltantesEmpresa().length;
  }

  get porcentaje() {
    return Math.round((this.completados / this.total) * 100);
  }

  get inicial() {
    return (this.form.nombreEmpresa || 'E').trim().charAt(0).toUpperCase();
  }

  elegirLogo(evento: Event) {
    const entrada = evento.target as HTMLInputElement;
    const archivo = entrada.files?.[0];
    entrada.value = '';
    this.errorLogo.set('');
    if (!archivo) return;

    if (!['image/png', 'image/jpeg', 'image/webp'].includes(archivo.type)) {
      this.errorLogo.set('El logo debe ser una imagen PNG, JPG o WEBP.');
      return;
    }
    if (archivo.size > 5 * 1024 * 1024) {
      this.errorLogo.set('La imagen no debe pasar de 5 MB.');
      return;
    }

    const lector = new FileReader();
    lector.onload = () => {
      const img = new Image();
      img.onload = () => {
        const escala = Math.min(1, 256 / Math.max(img.width, img.height));
        const ancho = Math.round(img.width * escala);
        const alto = Math.round(img.height * escala);
        const lienzo = document.createElement('canvas');
        lienzo.width = ancho;
        lienzo.height = alto;
        lienzo.getContext('2d')!.drawImage(img, 0, 0, ancho, alto);
        const url = lienzo.toDataURL('image/png');
        logoEmpresa.set(url);
        try { localStorage.setItem(CLAVE_LOGO, url); } catch {}
      };
      img.onerror = () => this.errorLogo.set('No se pudo leer la imagen.');
      img.src = lector.result as string;
    };
    lector.readAsDataURL(archivo);
  }

  quitarLogo() {
    logoEmpresa.set('');
    try { localStorage.removeItem(CLAVE_LOGO); } catch {}
  }

  guardar() {
    const e: Record<string, string> = {};

    if (this.form.nit.trim() && this.form.nit.replace(/\D/g, '').length !== 14) {
      e['nit'] = 'El NIT debe tener 14 dígitos.';
    }
    if (this.form.telefono.trim() && !TELEFONO.test(this.form.telefono.trim())) {
      e['telefono'] = 'Escribe un teléfono válido (de 8 a 15 dígitos).';
    }

    this.errores.set(e);
    this.mensaje.set('');
    if (Object.keys(e).length > 0) return;

    const limpio = { ...this.form };
    (Object.keys(limpio) as (keyof DatosEmpresa)[]).forEach(k => (limpio[k] = limpio[k].trim()));

    datosEmpresa.set(limpio);
    try { localStorage.setItem(CLAVE, JSON.stringify(limpio)); } catch {}
    this.form = { ...limpio };

    this.mensaje.set(
      perfilEmpresaCompleto()
        ? 'Perfil completo. Ya puedes publicar ofertas.'
        : `Datos guardados. Aún faltan ${faltantesEmpresa().length} campos obligatorios.`
    );
  }
}