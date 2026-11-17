import { Component, computed, signal } from '@angular/core';
import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Experiencia {
  puesto: string;
  empresa: string;
  desde: string;
  hasta: string | null; // null = puesto actual
}

export type Condicion = 'Graduado' | 'Egresado';

export interface Perfil {
  id: number;
  nombre: string;
  correo: string;
  condicion: Condicion;
  anio: number; // año de graduación (graduado) o de egreso (egresado)
  areaEspecializacion: string;
  empleado: boolean;
  puestoActual: string;
  empresaActual: string;
  sector: string;
  inicioPuestoActual: string;
  tecnologias: string[];
  perfilCompleto: boolean;
  historial: Experiencia[];
}

interface Dato {
  etiqueta: string;
  valor: number;
  porcentaje: number;
}

@Component({
  selector: 'app-seguimiento-admin',
  imports: [FormsModule, DatePipe, NgTemplateOutlet],
  templateUrl: './seguimiento-admin.html',
  styleUrl: './seguimiento-admin.css'
})
export class SeguimientoAdmin {
  // Se llenará con los datos que devuelva la API
  perfiles = signal<Perfil[]>([



  ]);

  vista = signal<'global' | 'persona'>('global');

  // ---------- Condición: graduados, egresados o todos ----------
  condicion = signal<Condicion | ''>('Graduado');

  cambiarCondicion(valor: Condicion | '') {
    this.condicion.set(valor);
    this.anio.set('');
    this.seleccionadoId.set(null);
    this.busqueda.set('');
  }

  etiqueta = computed(() => {
    const c = this.condicion();
    return c === 'Graduado' ? 'Graduados' : c === 'Egresado' ? 'Egresados' : 'Perfiles';
  });

  textoAnio = computed(() => {
    const c = this.condicion();
    return c === 'Graduado' ? 'año de graduación'
         : c === 'Egresado' ? 'año de egreso'
         : 'año de graduación o egreso';
  });

  private porCondicion = computed(() =>
    this.condicion()
      ? this.perfiles().filter(p => p.condicion === this.condicion())
      : this.perfiles()
  );

  // ---------- Vista global ----------
  anio = signal('');

  anios = computed(() =>
    [...new Set(this.porCondicion().map(p => p.anio))].sort((a, b) => b - a)
  );

  filtrados = computed(() =>
    this.anio()
      ? this.porCondicion().filter(p => String(p.anio) === this.anio())
      : this.porCondicion()
  );

  total = computed(() => this.filtrados().length);
  conEmpleo = computed(() => this.filtrados().filter(p => p.empleado).length);
  sinEmpleo = computed(() => this.total() - this.conEmpleo());
  porcentajeEmpleo = computed(() =>
    this.total() ? Math.round((this.conEmpleo() / this.total()) * 100) : 0
  );
  gradiente = computed(
    () => `conic-gradient(#14653a 0 ${this.porcentajeEmpleo()}%, #404c60 0)`
  );

  perfilesCompletos = computed(() => this.filtrados().filter(p => p.perfilCompleto).length);
  porcentajePerfiles = computed(() =>
    this.total() ? Math.round((this.perfilesCompletos() / this.total()) * 100) : 0
  );

  promedioPuesto = computed(() => {
    const empleados = this.filtrados().filter(p => p.empleado && p.inicioPuestoActual);
    if (!empleados.length) return '—';
    const suma = empleados.reduce((t, p) => t + this.aniosEnPuesto(p), 0);
    return this.formatoTiempo(suma / empleados.length);
  });

  porArea = computed(() => this.conteo(p => p.areaEspecializacion));
  porSector = computed(() => this.conteo(p => (p.empleado ? p.sector : '')));
  porAnio = computed(() =>
    this.conteo(p => String(p.anio)).sort((a, b) => a.etiqueta.localeCompare(b.etiqueta))
  );

  tecnologias = computed(() => {
    const mapa = new Map<string, number>();
    for (const p of this.filtrados()) {
      for (const t of p.tecnologias) {
        mapa.set(t, (mapa.get(t) ?? 0) + 1);
      }
    }
    const lista = [...mapa].map(([etiqueta, valor]) => ({ etiqueta, valor }));
    return this.conBarra(lista.sort((a, b) => b.valor - a.valor).slice(0, 8));
  });

  tiempoPuesto = computed(() => {
    const rangos = [
      { etiqueta: 'Menos de 1 año', valor: 0 },
      { etiqueta: 'De 1 a 3 años', valor: 0 },
      { etiqueta: 'De 3 a 5 años', valor: 0 },
      { etiqueta: 'Más de 5 años', valor: 0 },
    ];
    for (const p of this.filtrados()) {
      if (!p.empleado || !p.inicioPuestoActual) continue;
      const a = this.aniosEnPuesto(p);
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
    return this.porCondicion()
      .filter(p =>
        !texto ||
        p.nombre.toLowerCase().includes(texto) ||
        p.correo.toLowerCase().includes(texto)
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

  tiempoEnPuesto(p: Perfil): string {
    return p.empleado && p.inicioPuestoActual
      ? this.formatoTiempo(this.aniosEnPuesto(p))
      : '—';
  }

  // ---------- Auxiliares ----------
  private aniosEnPuesto(p: Perfil): number {
    const ms = Date.now() - new Date(p.inicioPuestoActual).getTime();
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