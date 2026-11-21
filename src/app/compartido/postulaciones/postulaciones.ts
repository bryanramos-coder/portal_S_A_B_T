import { signal } from '@angular/core';
import { datosAlumno, situacionAlumno, Experiencia } from '../../paginas/alumno/perfil-alumno/perfil-alumno';

// Estados de estado_postulacion
export type EstadoPostulacion = 'Recibida' | 'En revisión' | 'Seleccionado' | 'Descartado' | 'Retirada';
export const ESTADOS_POSTULACION: EstadoPostulacion[] =
  ['Recibida', 'En revisión', 'Seleccionado', 'Descartado', 'Retirada'];

// usuario.correo del alumno: con la API vendrá de la sesión
export const CORREO_ALUMNO = 'alumno@correo.com';

// postulacion (v5) junto con los datos que el alumno comparte con la empresa al aplicar.
// Con la API, la empresa consultará estos datos del alumno en el servidor.
export interface Postulacion {
  id: number;                 // id_postulacion
  idOferta: number;           // id_oferta
  puesto: string;
  empresa: string;
  vacantes: number;           // puestos_vacantes de la oferta
  nombres: string;
  apellidos: string;
  correo: string;
  telefono: string;
  situacion: string;
  anio: string;
  area: string;
  habilidades: string[];
  experiencias: Experiencia[];
  estado: EstadoPostulacion;  // id_estado_postulacion
  fechaPostulacion: string;   // fecha_postulacion (ISO)
  fechaResolucion: string;    // fecha_resolucion (ISO); vacía mientras sigue en curso
}

const CLAVE = 'postulaciones';

function leer(): Postulacion[] {
  try {
    const g = localStorage.getItem(CLAVE);
    return g ? JSON.parse(g) : [];
  } catch {
    return [];
  }
}

export const postulaciones = signal<Postulacion[]>(leer());

function guardar(lista: Postulacion[]) {
  postulaciones.set(lista);
  try { localStorage.setItem(CLAVE, JSON.stringify(lista)); } catch {}
}

const ahora = () => new Date().toISOString();

export function postulacionDe(idOferta: number): Postulacion | null {
  return postulaciones().find(p => p.idOferta === idOferta) ?? null;
}

// Vacantes ocupadas: postulantes Seleccionados (puestos_cubiertos)
export function cubiertasDe(idOferta: number): number {
  return postulaciones().filter(p => p.idOferta === idOferta && p.estado === 'Seleccionado').length;
}

// Postulantes activos de una oferta (sin contar los que retiraron su postulación)
export function postulantesDe(idOferta: number): number {
  return postulaciones().filter(p => p.idOferta === idOferta && p.estado !== 'Retirada').length;
}

// Aplicar o volver a aplicar. Hay una sola fila por (oferta, alumno): si el alumno la había retirado,
// se reutiliza y vuelve a Recibida.
export function postular(o: { id: number; puesto: string; empresa: string; vacantes: number }) {
  const d = datosAlumno();
  const datos = {
    idOferta: o.id, puesto: o.puesto, empresa: o.empresa, vacantes: o.vacantes,
    nombres: d.nombres, apellidos: d.apellidos, correo: CORREO_ALUMNO, telefono: d.telefono,
    situacion: situacionAlumno(), anio: d.anio, area: d.areaEsp,
    habilidades: [...d.habilidades], experiencias: JSON.parse(JSON.stringify(d.experiencias)),
  };
  const existente = postulacionDe(o.id);
  if (existente) {
    if (existente.estado !== 'Retirada') return;
    guardar(postulaciones().map(p => p.id === existente.id
      ? { ...p, ...datos, estado: 'Recibida' as const, fechaPostulacion: ahora(), fechaResolucion: '' }
      : p));
    return;
  }
  const id = postulaciones().reduce((m, p) => Math.max(m, p.id), 0) + 1;
  guardar([...postulaciones(), { id, ...datos, estado: 'Recibida', fechaPostulacion: ahora(), fechaResolucion: '' }]);
}

export function puedeRetirar(p: Postulacion): boolean {
  return p.estado === 'Recibida' || p.estado === 'En revisión';
}

export function retirarPostulacion(id: number) {
  const p = postulaciones().find(x => x.id === id);
  if (!p || !puedeRetirar(p)) return;
  guardar(postulaciones().map(x => x.id === id ? { ...x, estado: 'Retirada', fechaResolucion: ahora() } : x));
}

// A qué estados puede pasar la empresa una postulación
export function transicionesEmpresa(estado: EstadoPostulacion): EstadoPostulacion[] {
  if (estado === 'Recibida') return ['En revisión', 'Seleccionado', 'Descartado'];
  if (estado === 'En revisión') return ['Seleccionado', 'Descartado'];
  return [];
}

// Devuelve false si no se pudo (por ejemplo, Seleccionar sin vacantes disponibles)
export function cambiarEstado(id: number, estado: EstadoPostulacion): boolean {
  const p = postulaciones().find(x => x.id === id);
  if (!p || !transicionesEmpresa(p.estado).includes(estado)) return false;
  if (estado === 'Seleccionado' && cubiertasDe(p.idOferta) >= p.vacantes) return false;
  const resuelta = estado === 'Seleccionado' || estado === 'Descartado';
  guardar(postulaciones().map(x => x.id === id
    ? { ...x, estado, fechaResolucion: resuelta ? ahora() : '' }
    : x));
  return true;
}

export function claseEstadoPostulacion(e: string): string {
  switch (e) {
    case 'Seleccionado': return 'habilitado';
    case 'Descartado': return 'rechazado';
    case 'Retirada': return 'deshabilitado';
    default: return 'pendiente'; // Recibida, En revisión
  }
}