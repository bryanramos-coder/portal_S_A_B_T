import { Component, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Oferta {
  id: number;
  puesto: string;
  empresa: string;
  areaEmpresa: string;
  pais: string;
  departamento: string;
  ubicacionDetalle: string;
  tipo: string;
  nivelExperiencia: string;
  vacantes: number;
  vehiculo: string;
  modalidad: string;
  salario: string;
  tecnologias: string;
  objetivo: string;
  funciones: string;
  requisitos: string;
  competencias: string;
  horarios: string;
  ofrecemos: string;
  contacto: string;
  fechaPublicacion: string;
  fechaExpiracion: string;
  estado: 'Pendiente' | 'Publicada' | 'Rechazada' | 'Retirada';
  origen: 'Empresa' | 'Administrador';
}

export type FormOferta = Omit<Oferta, 'id' | 'estado' | 'origen' | 'fechaPublicacion'>;

function formVacio(): FormOferta {
  return {
    puesto: '', empresa: '', areaEmpresa: '',
    pais: 'El Salvador', departamento: '', ubicacionDetalle: '',
    tipo: '', nivelExperiencia: '', vacantes: 1, vehiculo: 'Indiferente',
    modalidad: 'Presencial', salario: '', tecnologias: '',
    objetivo: '', funciones: '', requisitos: '', competencias: '',
    horarios: '', ofrecemos: '', contacto: '', fechaExpiracion: '',
  };
}

@Component({
  selector: 'app-ofertas-admin',
  imports: [FormsModule, DatePipe],
  templateUrl: './ofertas-admin.html',
  styleUrl: './ofertas-admin.css'
})
export class OfertasAdmin {
  // Listas de opciones (la API puede devolverlas más adelante)
  departamentos = [
    'Ahuachapán', 'Santa Ana', 'Sonsonate', 'Chalatenango', 'La Libertad',
    'San Salvador', 'Cuscatlán', 'La Paz', 'Cabañas', 'San Vicente',
    'Usulután', 'San Miguel', 'Morazán', 'La Unión',
  ];
  areas = [
    'Bancos | Financieras', 'Tecnología | Informática', 'Telecomunicaciones',
    'Comercio | Ventas', 'Industria | Manufactura', 'Educación', 'Salud',
    'Servicios', 'Otra',
  ];
  tipos = ['Tiempo completo', 'Medio tiempo', 'Pasantía', 'Por proyecto'];
  niveles = ['Sin experiencia', 'Menos de un año', '1 a 2 años', '3 a 5 años', 'Más de 5 años'];
  vehiculos = ['Indiferente', 'Motocicleta', 'Automóvil'];
  modalidades = ['Presencial', 'Remoto', 'Híbrido'];

  hoy = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);

  // Se llenará con los datos que devuelva la API
  ofertas = signal<Oferta[]>([

  ]);

  busqueda = signal('');
  filtroEstado = signal('');
  filtroTipo = signal('');

  filtradas = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    return this.ofertas().filter(o =>
      (!texto ||
        o.puesto.toLowerCase().includes(texto) ||
        o.empresa.toLowerCase().includes(texto) ||
        o.tecnologias.toLowerCase().includes(texto)) &&
      (!this.filtroEstado() || o.estado === this.filtroEstado()) &&
      (!this.filtroTipo() || o.tipo === this.filtroTipo())
    );
  });

  limpiarFiltros() {
    this.busqueda.set('');
    this.filtroEstado.set('');
    this.filtroTipo.set('');
  }

  lineas(texto: string): string[] {
    return (texto || '').split('\n').map(l => l.trim()).filter(l => l);
  }

  // Ventana de detalle
  seleccionada = signal<Oferta | null>(null);
  rechazando = signal(false);
  motivo = signal('');
  errorMotivo = signal(false);
  confirmandoEliminar = signal(false);

  ver(oferta: Oferta) {
    this.seleccionada.set(oferta);
    this.rechazando.set(false);
    this.confirmandoEliminar.set(false);
    this.motivo.set('');
    this.errorMotivo.set(false);
  }

  cerrar() { this.seleccionada.set(null); }
  empezarRechazo() { this.rechazando.set(true); }

  cancelarRechazo() {
    this.rechazando.set(false);
    this.errorMotivo.set(false);
  }

  confirmarRechazo() {
    const oferta = this.seleccionada();
    if (!oferta) return;
    if (!this.motivo().trim()) {
      this.errorMotivo.set(true);
      return;
    }
    this.rechazar(oferta, this.motivo().trim());
    this.cerrar();
  }

  aprobarDesdeDetalle() {
    const oferta = this.seleccionada();
    if (!oferta) return;
    this.publicar(oferta);
    this.cerrar();
  }

  retirarDesdeDetalle() {
    const oferta = this.seleccionada();
    if (!oferta) return;
    this.retirar(oferta);
    this.cerrar();
  }

  eliminarDesdeDetalle() {
    const oferta = this.seleccionada();
    if (!oferta) return;
    this.eliminar(oferta);
    this.cerrar();
  }

  // Formulario de crear o editar
  formAbierto = signal(false);
  editandoId: number | null = null;
  form: FormOferta = formVacio();
  errores = signal<Record<string, string>>({});

  nueva() {
    this.editandoId = null;
    this.form = formVacio();
    this.errores.set({});
    this.formAbierto.set(true);
  }

  editar(oferta: Oferta) {
    this.cerrar();
    this.editandoId = oferta.id;
    const { id, estado, origen, fechaPublicacion, ...datos } = oferta;
    this.form = { ...datos };
    this.errores.set({});
    this.formAbierto.set(true);
  }

  cerrarFormulario() { this.formAbierto.set(false); }

  private validar(): Record<string, string> {
    const f = this.form;
    const e: Record<string, string> = {};
    const vacio = (v: unknown) => !String(v ?? '').trim();

    if (vacio(f.puesto)) e['puesto'] = 'Escribe el puesto.';
    if (vacio(f.empresa)) e['empresa'] = 'Escribe el nombre de la empresa.';
    if (vacio(f.areaEmpresa)) e['areaEmpresa'] = 'Selecciona el área de la empresa.';
    if (vacio(f.pais)) e['pais'] = 'Escribe el país.';
    if (vacio(f.departamento)) e['departamento'] = 'Selecciona el departamento.';
    if (vacio(f.tipo)) e['tipo'] = 'Selecciona el tipo de contratación.';
    if (vacio(f.nivelExperiencia)) e['nivelExperiencia'] = 'Selecciona el nivel de experiencia.';
    if (!f.vacantes || f.vacantes < 1) e['vacantes'] = 'Indica al menos 1 vacante.';
    if (vacio(f.fechaExpiracion)) {
      e['fechaExpiracion'] = 'Selecciona la fecha de expiración.';
    } else if (f.fechaExpiracion <= this.hoy) {
      e['fechaExpiracion'] = 'La fecha debe ser posterior a hoy.';
    }
    if (vacio(f.objetivo)) e['objetivo'] = 'Describe el objetivo del puesto.';
    if (vacio(f.funciones)) e['funciones'] = 'Escribe al menos una función.';
    if (vacio(f.requisitos)) e['requisitos'] = 'Escribe al menos un requisito.';
    return e;
  }

  guardar() {
    const e = this.validar();
    this.errores.set(e);
    if (Object.keys(e).length > 0) return;
    this.guardarOferta(this.form, this.editandoId);
    this.cerrarFormulario();
  }
    hayErrores() {
    return Object.keys(this.errores()).length > 0;
  }

  // Estas acciones llamarán a la API más adelante
  guardarOferta(datos: FormOferta, id: number | null) {}
  publicar(oferta: Oferta) {}
  rechazar(oferta: Oferta, motivo: string) {}
  retirar(oferta: Oferta) {}
  eliminar(oferta: Oferta) {}
}