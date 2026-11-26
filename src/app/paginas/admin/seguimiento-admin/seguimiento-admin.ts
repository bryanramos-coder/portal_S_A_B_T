import { Component, computed, signal } from '@angular/core';
import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Una fila de experiencia_laboral
export interface Experiencia {
  cargo: string;
  empresa: string;
  sector: string;             // vacío = sin indicar (id_sector es opcional)
  responsabilidades: string;
  desde: string;              // fecha_inicio, AAAA-MM-DD
  hasta: string | null;       // fecha_fin; null = empleo actual (es_actual)
}

// situacion_academica (catálogo): 1 Quinto año, 2 Egresado, 3 Graduado
export type Situacion = 'Quinto año' | 'Egresado' | 'Graduado';

// Un alumno con sus datos de registro, su área, habilidades y experiencias
export interface Perfil {
  id: number;
  nombres: string;
  apellidos: string;
  correo: string;
  telefono: string;
  carnet: string;
  situacion: Situacion;
  anio: number | null;        // anio_egreso_graduacion; vacío en quinto año
  areaEsp: string;            // vacío = sin área (id_area_esp es opcional)
  habilidades: string[];
  experiencias: Experiencia[];
}

interface Dato {
  etiqueta: string;
  valor: number;
  porcentaje: number;
}

// =====================================================================
// DATOS DE PRUEBA: quitar este bloque (y dejar signal<Perfil[]>([]))
// cuando la pantalla se conecte a la API.
// =====================================================================
const PERFILES_PRUEBA: Perfil[] = [
  {
    id: 1, nombres: 'María', apellidos: 'López', correo: 'maria.lopez@correo.com',
    telefono: '+503 7123-4567', carnet: 'RC23011', situacion: 'Graduado', anio: 2023,
    areaEsp: 'Desarrollo de software', habilidades: ['Angular', 'C#', 'PostgreSQL', 'Git'],
    experiencias: [
      { cargo: 'Desarrollador junior', empresa: 'Tecnosoft S.A.', sector: 'Tecnología | Informática',
        responsabilidades: 'Desarrollo de funcionalidades web.', desde: '2025-05-01', hasta: null },
      { cargo: 'Practicante de soporte técnico', empresa: 'Banco de América Central', sector: 'Bancos | Financieras',
        responsabilidades: 'Soporte a usuarios internos.', desde: '2024-01-01', hasta: '2025-04-30' },
    ],
  },
  {
    id: 2, nombres: 'Carlos', apellidos: 'Martínez', correo: 'carlos.m@correo.com',
    telefono: '+503 7222-1111', carnet: 'CM22045', situacion: 'Graduado', anio: 2022,
    areaEsp: 'Redes y telecomunicaciones', habilidades: ['Cisco', 'Linux', 'Redes'],
    experiencias: [
      { cargo: 'Técnico de redes', empresa: 'Claro El Salvador', sector: 'Telecomunicaciones',
        responsabilidades: 'Monitoreo y mantenimiento de la red.', desde: '2022-09-01', hasta: null },
    ],
  },
  {
    id: 3, nombres: 'Ana', apellidos: 'Rivas', correo: 'ana.rivas@correo.com',
    telefono: '+503 7333-2222', carnet: 'AR23012', situacion: 'Graduado', anio: 2023,
    areaEsp: 'Bases de datos', habilidades: ['PostgreSQL', 'SQL Server', 'Excel', 'Power BI'],
    experiencias: [
      { cargo: 'Analista de datos', empresa: 'Banco Agrícola', sector: 'Bancos | Financieras',
        responsabilidades: 'Reportes y tableros de control.', desde: '2024-03-01', hasta: null },
      { cargo: 'Auxiliar administrativo', empresa: 'Distribuidora ABC', sector: '',
        responsabilidades: 'Apoyo administrativo.', desde: '2023-02-01', hasta: '2024-02-28' },
    ],
  },
  {
    id: 4, nombres: 'Luis', apellidos: 'Hernández', correo: 'luis.h@correo.com',
    telefono: '+503 7444-3333', carnet: 'LH21078', situacion: 'Graduado', anio: 2021,
    areaEsp: '', habilidades: ['Java', 'MySQL'],
    experiencias: [
      { cargo: 'Programador', empresa: 'Soluciones Web', sector: '',
        responsabilidades: 'Desarrollo de sitios web.', desde: '2021-08-01', hasta: '2023-06-30' },
    ],
  },
  {
    id: 5, nombres: 'Sofía', apellidos: 'Pineda', correo: 'sofia.p@correo.com',
    telefono: '+503 7555-4444', carnet: 'SP20033', situacion: 'Graduado', anio: 2020,
    areaEsp: 'Desarrollo de software', habilidades: ['Angular', 'C#', 'Docker', 'Azure'],
    experiencias: [
      { cargo: 'Líder de desarrollo', empresa: 'Tecnosoft S.A.', sector: 'Tecnología | Informática',
        responsabilidades: 'Coordinación del equipo de desarrollo.', desde: '2020-10-01', hasta: null },
    ],
  },
  {
    id: 6, nombres: 'Diego', apellidos: 'Campos', correo: 'diego.c@correo.com',
    telefono: '+503 7666-5555', carnet: 'DC25101', situacion: 'Egresado', anio: 2025,
    areaEsp: 'Desarrollo de software', habilidades: ['angular', 'TypeScript', 'Git'],
    experiencias: [
      { cargo: 'Pasante de desarrollo', empresa: 'Tecnosoft S.A.', sector: 'Tecnología | Informática',
        responsabilidades: 'Apoyo en desarrollo de interfaces.', desde: '2026-06-01', hasta: null },
    ],
  },
  {
    id: 7, nombres: 'Karla', apellidos: 'Mejía', correo: 'karla.m@correo.com',
    telefono: '', carnet: 'KM26007', situacion: 'Egresado', anio: 2026,
    areaEsp: 'Bases de datos', habilidades: ['PostgreSQL', 'SQL'],
    experiencias: [],
  },
  // Quinto año: no aparece en el seguimiento (solo graduados y egresados)
  {
    id: 8, nombres: 'Mario', apellidos: 'Quintanilla', correo: 'mario.q@correo.com',
    telefono: '+503 7777-6666', carnet: 'MQ21190', situacion: 'Quinto año', anio: null,
    areaEsp: '', habilidades: ['Python'],
    experiencias: [],
  },
];

@Component({
  selector: 'app-seguimiento-admin',
  imports: [FormsModule, DatePipe, NgTemplateOutlet],
  templateUrl: './seguimiento-admin.html',
  styleUrl: './seguimiento-admin.css'
})
export class SeguimientoAdmin {
  // Se llenará con los datos que devuelva la API (vw_seguimiento_alumnos + habilidades + experiencias)
  perfiles = signal<Perfil[]>(PERFILES_PRUEBA);

  vista = signal<'global' | 'persona'>('global');

  // ---------- Situación: graduados, egresados o todos ----------
  // "Todos" = graduados y egresados. Quinto año no entra al seguimiento.
  situacion = signal<'Graduado' | 'Egresado' | ''>('Graduado');

  cambiarSituacion(valor: 'Graduado' | 'Egresado' | '') {
    this.situacion.set(valor);
    this.anio.set('');
    this.seleccionadoId.set(null);
    this.busqueda.set('');
  }

  etiqueta = computed(() => {
    const s = this.situacion();
    return s === 'Graduado' ? 'Graduados' : s === 'Egresado' ? 'Egresados' : 'Perfiles';
  });

  textoAnio = computed(() => {
    const s = this.situacion();
    return s === 'Graduado' ? 'año de graduación'
         : s === 'Egresado' ? 'año de egreso'
         : 'año de graduación o egreso';
  });

  private porSituacion = computed(() =>
    this.perfiles().filter(p =>
      this.situacion() ? p.situacion === this.situacion() : p.situacion !== 'Quinto año'
    )
  );

  // ---------- Vista global ----------
  anio = signal('');

  anios = computed(() =>
    [...new Set(
      this.porSituacion().map(p => p.anio).filter((a): a is number => a !== null)
    )].sort((a, b) => b - a)
  );

  filtrados = computed(() =>
    this.anio()
      ? this.porSituacion().filter(p => String(p.anio) === this.anio())
      : this.porSituacion()
  );

  total = computed(() => this.filtrados().length);
  conEmpleo = computed(() => this.filtrados().filter(p => this.empleoActual(p)).length);
  sinEmpleo = computed(() => this.total() - this.conEmpleo());
  porcentajeEmpleo = computed(() =>
    this.total() ? Math.round((this.conEmpleo() / this.total()) * 100) : 0
  );
  gradiente = computed(
    () => `conic-gradient(#14653a 0 ${this.porcentajeEmpleo()}%, #404c60 0)`
  );

  perfilesCompletos = computed(() => this.filtrados().filter(p => this.esCompleto(p)).length);
  porcentajePerfiles = computed(() =>
    this.total() ? Math.round((this.perfilesCompletos() / this.total()) * 100) : 0
  );

  promedioPuesto = computed(() => {
    const empleos = this.filtrados()
      .map(p => this.empleoActual(p))
      .filter((e): e is Experiencia => e !== null);
    if (!empleos.length) return '—';
    const suma = empleos.reduce((t, e) => t + this.aniosDesde(e.desde), 0);
    return this.formatoTiempo(suma / empleos.length);
  });

  porArea = computed(() => this.conteo(p => p.areaEsp || 'Sin área'));

  porSector = computed(() =>
    this.conteo(p => {
      const e = this.empleoActual(p);
      return e ? (e.sector || 'Sin sector') : '';
    })
  );

  porAnio = computed(() =>
    this.conteo(p => (p.anio === null ? '' : String(p.anio)))
      .sort((a, b) => a.etiqueta.localeCompare(b.etiqueta))
  );

  // Las habilidades se agrupan sin distinguir mayúsculas ("Angular" = "angular")
  tecnologias = computed(() => {
    const mapa = new Map<string, { etiqueta: string; valor: number }>();
    for (const p of this.filtrados()) {
      const vistas = new Set<string>();
      for (const h of p.habilidades) {
        const texto = h.trim();
        const clave = texto.toLowerCase();
        if (!clave || vistas.has(clave)) continue;
        vistas.add(clave);
        const existente = mapa.get(clave);
        if (existente) existente.valor++;
        else mapa.set(clave, { etiqueta: texto, valor: 1 });
      }
    }
    const lista = [...mapa.values()].sort((a, b) => b.valor - a.valor).slice(0, 8);
    return this.conBarra(lista);
  });

  tiempoPuesto = computed(() => {
    const rangos = [
      { etiqueta: 'Menos de 1 año', valor: 0 },
      { etiqueta: 'De 1 a 3 años', valor: 0 },
      { etiqueta: 'De 3 a 5 años', valor: 0 },
      { etiqueta: 'Más de 5 años', valor: 0 },
    ];
    for (const p of this.filtrados()) {
      const e = this.empleoActual(p);
      if (!e) continue;
      const a = this.aniosDesde(e.desde);
      if (a < 1) rangos[0].valor++;
      else if (a < 3) rangos[1].valor++;
      else if (a < 5) rangos[2].valor++;
      else rangos[3].valor++;
    }
    return this.conBarra(rangos);
  });

  // ---------- Vista por persona ----------
  busqueda = signal('');
  seleccionadoId = signal<number | null>(null);

  resultados = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    return this.porSituacion()
      .filter(p =>
        !texto ||
        this.nombreCompleto(p).toLowerCase().includes(texto) ||
        p.correo.toLowerCase().includes(texto) ||
        p.carnet.toLowerCase().includes(texto)
      )
      .slice(0, 8);
  });

  seleccionado = computed(
    () => this.perfiles().find(p => p.id === this.seleccionadoId()) ?? null
  );

  elegir(perfil: Perfil) {
    this.seleccionadoId.set(perfil.id);
  }

  volver() {
    this.seleccionadoId.set(null);
  }

  // ---------- Reglas sobre un perfil ----------
  nombreCompleto(p: Perfil): string {
    return `${p.nombres} ${p.apellidos}`.trim();
  }

  // Se permiten varios empleos actuales; se toma el que empezó más recientemente
  empleoActual(p: Perfil): Experiencia | null {
    const actuales = p.experiencias.filter(x => !x.hasta);
    if (!actuales.length) return null;
    return actuales.reduce((a, b) => (b.desde > a.desde ? b : a));
  }

  historial(p: Perfil): Experiencia[] {
    return [...p.experiencias].sort((a, b) => b.desde.localeCompare(a.desde));
  }

  tiempoEnPuesto(p: Perfil): string {
    const e = this.empleoActual(p);
    return e ? this.formatoTiempo(this.aniosDesde(e.desde)) : '—';
  }

  // Misma regla que Mi perfil del alumno
  esCompleto(p: Perfil): boolean {
    return !!(
      p.nombres.trim() && p.apellidos.trim() && p.telefono.trim() && p.carnet.trim() &&
      p.anio !== null && (p.situacion !== 'Graduado' || p.areaEsp)
    );
  }

  etiquetaAnio(p: Perfil): string {
    return p.situacion === 'Graduado' ? 'Año de graduación' : 'Año de egreso';
  }

  // ---------- Descargas (RF-20) ----------
  generado = signal('');

  // PDF: abre la ventana de impresión; ahí se elige "Guardar como PDF"
  imprimir() {
    this.generado.set(this.fechaTexto());
    setTimeout(() => window.print(), 100);
  }

  descargarGlobal() {
    if (!this.total()) return;
    const f: string[] = [];
    const t = this.total();

    f.push(this.fila('Seguimiento de graduados - Reporte global'));
    f.push(this.fila('Situación', this.etiqueta()));
    f.push(this.fila('Año de graduación o egreso', this.anio() || 'Todos'));
    f.push(this.fila('Generado', this.fechaTexto()));
    f.push('');

    f.push(this.fila('INDICADORES'));
    f.push(this.fila('Indicador', 'Cantidad', 'Porcentaje'));
    f.push(this.fila(`${this.etiqueta()} registrados`, t, ''));
    f.push(this.fila('Con empleo actual', this.conEmpleo(), this.porcentajeEmpleo() + '%'));
    f.push(this.fila('Sin empleo registrado', this.sinEmpleo(), Math.round((this.sinEmpleo() / t) * 100) + '%'));
    f.push(this.fila('Tiempo promedio en el puesto', this.promedioPuesto(), ''));
    f.push(this.fila('Perfiles completos', this.perfilesCompletos(), this.porcentajePerfiles() + '%'));
    f.push('');

    this.bloque(f, 'POR ÁREA DE ESPECIALIZACIÓN', this.porArea());
    this.bloque(f, 'EMPLEO POR SECTOR', this.porSector());
    this.bloque(f, 'TECNOLOGÍAS MÁS USADAS', this.tecnologias());
    this.bloque(f, 'POR AÑO', this.porAnio());
    this.bloque(f, 'TIEMPO EN EL PUESTO ACTUAL', this.tiempoPuesto());

    f.push(this.fila('DETALLE POR PERSONA'));
    f.push(this.fila(
      'Nombres', 'Apellidos', 'Correo', 'Carnet', 'Situación', 'Año', 'Área de especialización',
      'Empleo actual', 'Empresa actual', 'Sector', 'Tiempo en el puesto', 'Perfil completo', 'Habilidades'
    ));
    for (const p of this.filtrados()) {
      const e = this.empleoActual(p);
      f.push(this.fila(
        p.nombres, p.apellidos, p.correo, p.carnet, p.situacion, p.anio ?? '',
        p.areaEsp || 'Sin área',
        e ? e.cargo : '', e ? e.empresa : '', e ? (e.sector || 'Sin sector') : '',
        this.tiempoEnPuesto(p), this.esCompleto(p) ? 'Sí' : 'No', p.habilidades.join(', ')
      ));
    }

    this.descargar(`seguimiento-global-${this.fechaArchivo()}.csv`, f);
  }

  descargarPersona(p: Perfil) {
    const e = this.empleoActual(p);
    const f: string[] = [];

    f.push(this.fila('Seguimiento de graduados - Ficha individual'));
    f.push(this.fila('Generado', this.fechaTexto()));
    f.push('');

    f.push(this.fila('DATOS'));
    f.push(this.fila('Nombres', p.nombres));
    f.push(this.fila('Apellidos', p.apellidos));
    f.push(this.fila('Correo', p.correo));
    f.push(this.fila('Carnet', p.carnet));
    f.push(this.fila('Situación', p.situacion));
    f.push(this.fila(this.etiquetaAnio(p), p.anio ?? ''));
    f.push(this.fila('Área de especialización', p.areaEsp || 'Sin área'));
    f.push(this.fila('Perfil', this.esCompleto(p) ? 'Completo' : 'Incompleto'));
    f.push(this.fila('Puesto actual', e ? e.cargo : ''));
    f.push(this.fila('Empresa actual', e ? e.empresa : ''));
    f.push(this.fila('Sector', e ? (e.sector || 'Sin sector') : ''));
    f.push(this.fila('Tiempo en el puesto', this.tiempoEnPuesto(p)));
    f.push('');

    f.push(this.fila('HABILIDADES'));
    f.push(this.fila(p.habilidades.join(', ') || 'Sin habilidades registradas'));
    f.push('');

    f.push(this.fila('HISTORIAL LABORAL'));
    f.push(this.fila('Cargo', 'Empresa', 'Sector', 'Desde', 'Hasta', 'Responsabilidades'));
    for (const x of this.historial(p)) {
      f.push(this.fila(
        x.cargo, x.empresa, x.sector || 'Sin sector',
        this.mesAnio(x.desde), x.hasta ? this.mesAnio(x.hasta) : 'Actual', x.responsabilidades
      ));
    }

    const carnet = p.carnet.replace(/[^A-Za-z0-9]/g, '');
    this.descargar(`seguimiento-${carnet || 'persona'}-${this.fechaArchivo()}.csv`, f);
  }

  private bloque(f: string[], titulo: string, datos: Dato[]) {
    f.push(this.fila(titulo));
    f.push(this.fila('Categoría', 'Cantidad'));
    if (!datos.length) f.push(this.fila('Sin datos', ''));
    for (const d of datos) f.push(this.fila(d.etiqueta, d.valor));
    f.push('');
  }

  // Una celda del archivo. Los textos que empiezan con = + - @ se neutralizan
  // para que Excel no los ejecute como fórmula (los datos los escriben los usuarios).
  private celda(v: string | number | null | undefined): string {
    let texto = v === null || v === undefined ? '' : String(v);
    texto = texto.replace(/\t/g, ' ');              // el tabulador es el separador de columnas
    if (/^[=+\-@\r]/.test(texto)) texto = "'" + texto;
    return '"' + texto.replace(/"/g, '""') + '"';
  }

  private fila(...valores: (string | number | null | undefined)[]): string {
    return valores.map(v => this.celda(v)).join('\t');
  }

  private descargar(nombre: string, filas: string[]) {
    // UTF-16 con marca BOM y columnas separadas por tabulador: Excel lo abre con doble clic,
    // lee bien las tildes y la ñ, y no depende del separador de listas de Windows.
    const contenido = filas.join('\r\n');
    const unidades = new Uint16Array(contenido.length + 1);
    unidades[0] = 0xFEFF;
    for (let i = 0; i < contenido.length; i++) unidades[i + 1] = contenido.charCodeAt(i);
    const blob = new Blob([unidades], { type: 'text/csv;charset=utf-16le' });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  private fechaTexto(): string {
    return new Date().toLocaleString('es-SV', { dateStyle: 'long', timeStyle: 'short' });
  }

  private fechaArchivo(): string {
    return new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);
  }

  private mesAnio(fecha: string): string {
    const [a, m] = fecha.split('-');
    return `${m}/${a}`;
  }

  // ---------- Auxiliares ----------
  private aniosDesde(desde: string): number {
    const ms = Date.now() - new Date(desde).getTime();
    return Math.max(0, ms / (1000 * 60 * 60 * 24 * 365.25));
  }

  private formatoTiempo(anios: number): string {
    if (anios < 1) {
      const meses = Math.max(1, Math.round(anios * 12));
      return meses + (meses === 1 ? ' mes' : ' meses');
    }
    const a = Math.round(anios * 10) / 10;
    return a.toLocaleString('es-SV') + (a === 1 ? ' año' : ' años');
  }

  private conBarra(datos: { etiqueta: string; valor: number }[]): Dato[] {
    const maximo = Math.max(1, ...datos.map(d => d.valor));
    return datos.map(d => ({ ...d, porcentaje: (d.valor / maximo) * 100 }));
  }

  private conteo(clave: (p: Perfil) => string): Dato[] {
    const mapa = new Map<string, number>();
    for (const p of this.filtrados()) {
      const k = clave(p);
      if (k) mapa.set(k, (mapa.get(k) ?? 0) + 1);
    }
    const lista = [...mapa].map(([etiqueta, valor]) => ({ etiqueta, valor }));
    return this.conBarra(lista.sort((a, b) => b.valor - a.valor));
  }
}