// Modelo y utilidades compartidas por las pantallas de ofertas (admin, empresa y alumno).
// Cada campo corresponde a una columna de oferta_laboral (v5).

export type EstadoOferta = 'Borrador' | 'Publicada' | 'Cubierta' | 'Vencida' | 'Cerrada';
export const ESTADOS_OFERTA: EstadoOferta[] = ['Borrador', 'Publicada', 'Cubierta', 'Vencida', 'Cerrada'];

export interface Oferta {
  id: number;
  puesto: string;             // titulo VARCHAR(150)
  empresa: string;            // nombre de la empresa (id_empresa) o empresa_externa VARCHAR(150)
  sector: string;             // id_sector (obligatorio)
  departamento: string;       // departamento VARCHAR(60)
  ubicacionDetalle: string;   // detalle_ubicacion VARCHAR(200), opcional
  tipo: string;               // tipo_empleo VARCHAR(20)
  nivelExperiencia: string;   // nivel_experiencia VARCHAR(50)
  vacantes: number;           // puestos_vacantes
  cubiertas: number;          // puestos_cubiertos (lo actualiza el sistema)
  vehiculo: string;           // vehiculo, opcional ('' = sin indicar)
  modalidad: string;          // modalidad, opcional ('' = sin indicar)
  salarioMin: number | null;  // salario_min NUMERIC(10,2), opcional
  salarioMax: number | null;  // salario_max NUMERIC(10,2), opcional
  habilidades: string[];      // oferta_habilidad (al menos una para publicar)
  objetivo: string;           // mision
  funciones: string;
  requisitos: string;
  competencias: string;       // opcional
  horarios: string;           // opcional
  ofrecemos: string;          // opcional
  contactoNombre: string;     // persona_contacto VARCHAR(150), opcional
  contactoCorreo: string;     // correo_contacto VARCHAR(150), opcional
  indicaciones: string;       // opcional
  fechaPublicacion: string;   // la asigna el sistema al publicar ('' en borrador)
  fechaExpiracion: string;    // fecha_vencimiento
  estado: EstadoOferta;
  origen: 'Empresa' | 'Administrador'; // quién la cargó (id_publicador)
}

export type FormOferta = Omit<Oferta, 'id' | 'estado' | 'origen' | 'fechaPublicacion' | 'cubiertas'>;

export const DEPARTAMENTOS = [
  'Ahuachapán', 'Santa Ana', 'Sonsonate', 'Chalatenango', 'La Libertad',
  'San Salvador', 'Cuscatlán', 'La Paz', 'Cabañas', 'San Vicente',
  'Usulután', 'San Miguel', 'Morazán', 'La Unión',
];
// Deben coincidir con el catálogo sector de la base de datos
export const SECTORES = [
  'Bancos | Financieras', 'Tecnología | Informática', 'Telecomunicaciones',
  'Comercio | Ventas', 'Industria | Manufactura', 'Educación', 'Salud',
  'Servicios', 'Otra',
];
export const TIPOS = ['Tiempo completo', 'Medio tiempo', 'Pasantía', 'Por proyecto'];
export const NIVELES = ['Sin experiencia', 'Menos de un año', '1 a 2 años', '3 a 5 años', 'Más de 5 años'];
export const VEHICULOS = ['Indiferente', 'Motocicleta', 'Automóvil'];
export const MODALIDADES = ['Presencial', 'Remoto', 'Híbrido'];

export function hoyLocal(): string {
  return new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

export function formVacio(): FormOferta {
  return {
    puesto: '', empresa: '', sector: '', departamento: '', ubicacionDetalle: '',
    tipo: '', nivelExperiencia: '', vacantes: 1, vehiculo: '', modalidad: '',
    salarioMin: null, salarioMax: null, habilidades: [],
    objetivo: '', funciones: '', requisitos: '', competencias: '', horarios: '', ofrecemos: '',
    contactoNombre: '', contactoCorreo: '', indicaciones: '', fechaExpiracion: '',
  };
}

export function datosDeOferta(o: Oferta): FormOferta {
  const { id, estado, origen, fechaPublicacion, cubiertas, ...datos } = o;
  return { ...datos, habilidades: [...datos.habilidades] };
}

export function limpiarOferta(f: FormOferta): FormOferta {
  const t = (s: string) => (s ?? '').trim();
  return {
    ...f,
    puesto: t(f.puesto), empresa: t(f.empresa), sector: f.sector, departamento: f.departamento,
    ubicacionDetalle: t(f.ubicacionDetalle),
    objetivo: t(f.objetivo), funciones: t(f.funciones), requisitos: t(f.requisitos),
    competencias: t(f.competencias), horarios: t(f.horarios), ofrecemos: t(f.ofrecemos),
    contactoNombre: t(f.contactoNombre), contactoCorreo: t(f.contactoCorreo), indicaciones: t(f.indicaciones),
    habilidades: [...f.habilidades],
  };
}

// Mismo criterio que habilidad.nombre: sin espacios sobrantes y sin repetir aunque cambien las mayúsculas
export function agregarHabilidad(lista: string[], texto: string): string[] {
  const h = (texto ?? '').trim().replace(/\s+/g, ' ');
  if (!h || lista.some(x => x.toLowerCase() === h.toLowerCase())) return lista;
  return [...lista, h];
}

export function claseEstadoOferta(e: string): string {
  switch (e) {
    case 'Borrador': return 'pendiente';
    case 'Publicada': return 'habilitado';
    case 'Vencida': return 'rechazado';
    default: return 'deshabilitado'; // Cubierta, Cerrada
  }
}

export function rangoSalario(min: number | null, max: number | null): string {
  const f = (n: number) => '$' + n.toLocaleString('en-US', { maximumFractionDigits: 2 });
  if (min != null && max != null) return `${f(min)} – ${f(max)}`;
  if (min != null) return `Desde ${f(min)}`;
  if (max != null) return `Hasta ${f(max)}`;
  return 'No indicado';
}

export function lineas(texto: string): string[] {
  return (texto || '').split('\n').map(l => l.trim()).filter(l => l);
}

// Validación común. externa = oferta de una empresa no registrada (la carga el administrador).
// publicar = además de los campos obligatorios, exige al menos una habilidad.
export function validarOferta(
  f: FormOferta, hoy: string, opc: { externa: boolean; publicar: boolean }
): Record<string, string> {
  const e: Record<string, string> = {};
  const vacio = (v: unknown) => !String(v ?? '').trim();
  const LIMITE = 99999999.99; // NUMERIC(10,2)

  if (vacio(f.puesto)) e['puesto'] = 'Escribe el puesto.';
  if (opc.externa && vacio(f.empresa)) e['empresa'] = 'Escribe el nombre de la empresa.';
  if (vacio(f.sector)) e['sector'] = 'Selecciona el sector.';
  if (vacio(f.departamento)) e['departamento'] = 'Selecciona el departamento.';
  if (vacio(f.tipo)) e['tipo'] = 'Selecciona el tipo de contratación.';
  if (vacio(f.nivelExperiencia)) e['nivelExperiencia'] = 'Selecciona el nivel de experiencia.';

  const v = Number(f.vacantes);
  if (!Number.isInteger(v) || v < 1 || v > 32767) e['vacantes'] = 'Indica un número entero de 1 a 32,767.';

  if (vacio(f.fechaExpiracion)) e['fechaExpiracion'] = 'Selecciona la fecha de expiración.';
  else if (f.fechaExpiracion <= hoy) e['fechaExpiracion'] = 'La fecha debe ser posterior a hoy.';

  if (vacio(f.objetivo)) e['objetivo'] = 'Describe el objetivo del puesto.';
  if (vacio(f.funciones)) e['funciones'] = 'Escribe al menos una función.';
  if (vacio(f.requisitos)) e['requisitos'] = 'Escribe al menos un requisito.';

  const min = f.salarioMin, max = f.salarioMax;
  if (min != null && (min < 0 || min > LIMITE)) e['salarioMin'] = 'Escribe un monto entre 0 y 99,999,999.99.';
  if (max != null && (max < 0 || max > LIMITE)) e['salarioMax'] = 'Escribe un monto entre 0 y 99,999,999.99.';
  if (min != null && max != null && max < min && !e['salarioMin'] && !e['salarioMax']) {
    e['salarioMax'] = 'El salario máximo no puede ser menor que el mínimo.';
  }

  if (!vacio(f.contactoCorreo) && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(f.contactoCorreo.trim())) {
    e['contactoCorreo'] = 'Escribe un correo válido.';
  }
  if (opc.publicar && f.habilidades.length === 0) {
    e['habilidades'] = 'Agrega al menos una habilidad para poder publicar.';
  }
  return e;
}