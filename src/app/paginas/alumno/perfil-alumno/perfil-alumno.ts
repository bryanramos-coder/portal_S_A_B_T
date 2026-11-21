import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SECTORES } from '../../../compartido/ofertas/ofertas-comun';

export type Situacion = 'Quinto año' | 'Egresado' | 'Graduado';

// experiencia_laboral
export interface Experiencia {
  cargo: string;             // cargo VARCHAR(100)
  empresa: string;           // empresa VARCHAR(150)
  sector: string;            // id_sector (opcional)
  responsabilidades: string; // responsabilidades TEXT
  desde: string;             // fecha_inicio (AAAA-MM; la API guarda el día 1)
  hasta: string;             // fecha_fin; vacío = empleo actual (es_actual)
}

export interface DatosAlumno {
  nombres: string;           // datos_registro.nombres VARCHAR(100)
  apellidos: string;         // datos_registro.apellidos VARCHAR(100)
  telefono: string;          // datos_registro.telefono VARCHAR(20)
  carnet: string;            // alumno.carnet VARCHAR(20)
  areaEsp: string;           // alumno.id_area_esp (opcional)
  anio: string;              // alumno.anio_egreso_graduacion; vacío en quinto año
  habilidades: string[];     // alumno_habilidad (habilidad.nombre VARCHAR(100))
  experiencias: Experiencia[];
  visible: boolean;          // consentimiento_visibilidad (vigente)
  visibleDesde: string;      // consentimiento_visibilidad.fecha_otorgado
}

const SITUACIONES: Situacion[] = ['Quinto año', 'Egresado', 'Graduado'];
const CLAVE_SITUACION = 'situacion-alumno';
const CLAVE = 'perfil-alumno';
const CLAVE_FOTO = 'foto-alumno';

const TELEFONO = /^\+?(?=(?:[^0-9]*[0-9]){8,15}$)[0-9][0-9\s\-]*[0-9]$/;
const CARNET = /^[A-Z]{2}[0-9]{3,6}$/;

const TIPOS_FOTO = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_FOTO = 5 * 1024 * 1024; // 5 MB del archivo original
const LADO_FOTO = 256;            // se reduce a 256 x 256 px

function fechaLocal(): string {
  return new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

function leerSituacion(): Situacion {
  try {
    const s = localStorage.getItem(CLAVE_SITUACION) as Situacion | null;
    return s && SITUACIONES.includes(s) ? s : 'Graduado';
  } catch {
    return 'Graduado';
  }
}

function leerFoto(): string | null {
  try {
    return localStorage.getItem(CLAVE_FOTO);
  } catch {
    return null;
  }
}

// Recorta la imagen a un cuadrado y la reduce para que pese poco
function reducirImagen(archivo: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const url = URL.createObjectURL(archivo);
    const img = new Image();
    img.onload = () => {
      const lado = Math.min(img.width, img.height);
      const sx = (img.width - lado) / 2;
      const sy = (img.height - lado) / 2;
      const canvas = document.createElement('canvas');
      canvas.width = LADO_FOTO;
      canvas.height = LADO_FOTO;
      const ctx = canvas.getContext('2d');
      if (!ctx) { URL.revokeObjectURL(url); reject(new Error('canvas')); return; }
      ctx.drawImage(img, sx, sy, lado, lado, 0, 0, LADO_FOTO, LADO_FOTO);
      URL.revokeObjectURL(url);
      resolve(canvas.toDataURL('image/jpeg', 0.85));
    };
    img.onerror = () => { URL.revokeObjectURL(url); reject(new Error('imagen')); };
    img.src = url;
  });
}

// alumno.id_situacion. La eligió en el registro; con la API vendrá de la sesión.
export const situacionAlumno = signal<Situacion>(leerSituacion());

// datos_registro.foto_url. Con la API será la URL de la imagen subida al servidor.
export const fotoAlumno = signal<string | null>(leerFoto());

function vacio(): DatosAlumno {
  return {
    nombres: '', apellidos: '', telefono: '', carnet: '', areaEsp: '', anio: '',
    habilidades: [], experiencias: [], visible: false, visibleDesde: '',
  };
}

// Datos que escribió en el registro. Con la API vendrán del servidor.
function datosDeRegistro(): Partial<DatosAlumno> {
  return {
    nombres: 'María', apellidos: 'López', telefono: '+503 7123-4567', carnet: 'RC23011',
    anio: situacionAlumno() === 'Quinto año' ? '' : '2023',
  };
}

function leer(): DatosAlumno {
  const base = { ...vacio(), ...datosDeRegistro() };
  try {
    const guardado = localStorage.getItem(CLAVE);
    return guardado ? { ...base, ...JSON.parse(guardado) } : base;
  } catch {
    return base;
  }
}

function clonar(d: DatosAlumno): DatosAlumno {
  return JSON.parse(JSON.stringify(d));
}

function etiquetaAnioDe(s: Situacion): string {
  return s === 'Graduado' ? 'Año de graduación' : 'Año de egreso';
}

export const datosAlumno = signal<DatosAlumno>(leer());

// Obligatorios de esta pantalla. La base de datos solo exige los del registro; el área de un
// graduado es la única regla de pantalla más estricta que la base. La foto es opcional.
export const faltantesAlumno = computed(() => {
  const d = datosAlumno();
  const s = situacionAlumno();
  const f: string[] = [];
  if (!d.nombres.trim()) f.push('nombres');
  if (!d.apellidos.trim()) f.push('apellidos');
  if (!d.telefono.trim()) f.push('teléfono');
  if (!d.carnet.trim()) f.push('carnet');
  if (s !== 'Quinto año' && !d.anio.trim()) f.push(etiquetaAnioDe(s).toLowerCase());
  if (s === 'Graduado' && !d.areaEsp) f.push('área de especialización');
  return f;
});

export const totalObligatorios = computed(() => {
  const s = situacionAlumno();
  return 4 + (s !== 'Quinto año' ? 1 : 0) + (s === 'Graduado' ? 1 : 0);
});

export const perfilAlumnoCompleto = computed(() => faltantesAlumno().length === 0);

@Component({
  selector: 'app-perfil-alumno',
  imports: [FormsModule],
  templateUrl: './perfil-alumno.html',
  styleUrl: './perfil-alumno.css'
})
export class PerfilAlumno {
  correo = 'alumno@correo.com'; // usuario.correo, vendrá de la sesión

  // Deben coincidir con los catálogos de la base de datos
  situaciones = SITUACIONES;
  areas = ['Desarrollo de software', 'Redes y telecomunicaciones', 'Bases de datos', 'Otra'];
   sectores = SECTORES;

  hoy = fechaLocal().slice(0, 7); // AAAA-MM

  situacion = situacionAlumno;
  faltantes = faltantesAlumno;
  completo = perfilAlumnoCompleto;
  total = totalObligatorios;

  form: DatosAlumno = clonar(datosAlumno());
  situacionSel: Situacion = situacionAlumno();
  fotoForm = signal<string | null>(fotoAlumno());
  errorFoto = signal('');
  nuevaHabilidad = '';
  errores = signal<Record<string, string>>({});
  mensaje = signal('');

  get completados() { return this.total() - this.faltantes().length; }
  get porcentaje() { return Math.round((this.completados / this.total()) * 100); }
  get esQuinto() { return this.situacionSel === 'Quinto año'; }
  get esGraduado() { return this.situacionSel === 'Graduado'; }
  get etiquetaAnio() { return etiquetaAnioDe(this.situacionSel); }

  get iniciales() {
    const n = this.form.nombres.trim().charAt(0);
    const a = this.form.apellidos.trim().charAt(0);
    return (n + a).toUpperCase() || '?';
  }

  get descripcionSituacion() {
    const s = this.situacionSel;
    return s === 'Graduado' ? 'Ya obtuviste tu título.'
         : s === 'Egresado' ? 'Terminaste tus materias y aún no te has graduado.'
         : 'Estás en quinto año o en trámite de graduación.';
  }

  get visibleDesdeTexto() {
    const f = datosAlumno().visibleDesde;
    if (!f) return '';
    const [a, m, d] = f.split('-');
    return `${d}/${m}/${a}`;
  }

  // En quinto año el año debe quedar vacío (CHECK de alumno.anio_egreso_graduacion)
  cambiarSituacion(s: Situacion) {
    this.situacionSel = s;
    if (s === 'Quinto año') this.form.anio = '';
  }

  async elegirFoto(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = ''; // permite volver a elegir el mismo archivo
    if (!archivo) return;

    if (!TIPOS_FOTO.includes(archivo.type)) {
      this.errorFoto.set('Elige una imagen JPG, PNG o WEBP.');
      return;
    }
    if (archivo.size > MAX_FOTO) {
      this.errorFoto.set('La imagen es muy pesada. El máximo es 5 MB.');
      return;
    }
    try {
      this.fotoForm.set(await reducirImagen(archivo));
      this.errorFoto.set('');
    } catch {
      this.errorFoto.set('No se pudo leer la imagen. Prueba con otra.');
    }
  }

  quitarFoto() {
    this.fotoForm.set(null);
    this.errorFoto.set('');
  }

  agregarHabilidad() {
    const h = this.nuevaHabilidad.trim().replace(/\s+/g, ' ');
    if (!h) return;
    if (!this.form.habilidades.some(x => x.toLowerCase() === h.toLowerCase())) {
      this.form.habilidades.push(h);
    }
    this.nuevaHabilidad = '';
  }

  quitarHabilidad(i: number) {
    this.form.habilidades.splice(i, 1);
  }

  agregarExperiencia() {
    this.form.experiencias.push({
      cargo: '', empresa: '', sector: '', responsabilidades: '', desde: '', hasta: '',
    });
  }

  quitarExperiencia(i: number) {
    this.form.experiencias.splice(i, 1);
  }

  guardar() {
    const f = this.form;
    const s = this.situacionSel;
    const e: Record<string, string> = {};

    if (f.telefono.trim() && !TELEFONO.test(f.telefono.trim())) {
      e['telefono'] = 'Escribe un teléfono válido (de 8 a 15 dígitos).';
    }

    const carnet = f.carnet.trim().toUpperCase();
    if (carnet && !CARNET.test(carnet)) {
      e['carnet'] = 'El carnet debe iniciar con dos letras seguidas de 3 a 6 números (Ej: RC23011).';
    }

    // CHECK de la base de datos: entre 1960 y 2100 (nulo en quinto año)
    const anio = f.anio.trim();
    if (s !== 'Quinto año' && anio) {
      const n = Number(anio);
      if (!/^\d{4}$/.test(anio) || n < 1960 || n > 2100) {
        e['anio'] = 'Escribe un año entre 1960 y 2100.';
      }
    }

    // Se permiten varios empleos actuales (la base de datos no lo restringe)
    f.experiencias.forEach((x, i) => {
      if (!x.cargo.trim() || !x.empresa.trim() || !x.responsabilidades.trim() || !x.desde) {
        e['exp-' + i] = 'Completa el cargo, la empresa, las responsabilidades y la fecha de inicio.';
      } else if (x.desde > this.hoy) {
        e['exp-' + i] = 'La fecha de inicio no puede ser futura.';
      } else if (x.hasta && x.hasta < x.desde) {
        e['exp-' + i] = 'La fecha final no puede ser anterior a la de inicio.';
      }
    });

    this.errores.set(e);
    this.mensaje.set('');
    if (Object.keys(e).length > 0) return;

    const anterior = datosAlumno();
    const limpio: DatosAlumno = {
      nombres: f.nombres.trim(),
      apellidos: f.apellidos.trim(),
      telefono: f.telefono.trim(),
      carnet,
      areaEsp: f.areaEsp,
      anio: s === 'Quinto año' ? '' : anio,
      habilidades: [...f.habilidades],
      experiencias: f.experiencias.map(x => ({
        cargo: x.cargo.trim(),
        empresa: x.empresa.trim(),
        sector: x.sector,
        responsabilidades: x.responsabilidades.trim(),
        desde: x.desde,
        hasta: x.hasta,
      })),
      visible: f.visible,
      visibleDesde: f.visible ? (anterior.visible ? anterior.visibleDesde : fechaLocal()) : '',
    };

    // Situación académica (alumno.id_situacion)
    situacionAlumno.set(s);
    try { localStorage.setItem(CLAVE_SITUACION, s); } catch {}

    datosAlumno.set(limpio);
    try { localStorage.setItem(CLAVE, JSON.stringify(limpio)); } catch {}
    this.form = clonar(limpio);

    // Foto (datos_registro.foto_url): la barra superior se actualiza al guardar
    const foto = this.fotoForm();
    fotoAlumno.set(foto);
    try {
      if (foto) localStorage.setItem(CLAVE_FOTO, foto);
      else localStorage.removeItem(CLAVE_FOTO);
    } catch {}

    this.mensaje.set(
      perfilAlumnoCompleto()
        ? 'Perfil completo. Datos guardados.'
        : `Datos guardados. Aún faltan ${faltantesAlumno().length} campos obligatorios.`
    );
  }
}