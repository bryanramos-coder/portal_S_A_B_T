import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface Usuario {
  id: number;
  nombre: string;            // alumno: nombres y apellidos; empresa: nombre_empresa
  correo: string;
  tipo: 'Alumno' | 'Empresa' | 'Administrador';
  estado: 'Pendiente' | 'Activa' | 'Suspendida' | 'Rechazada';  fechaRegistro: string;
  telefono?: string;         // datos_registro.telefono

  // Solo alumnos
  carnet?: string;
  situacion?: 'Quinto año' | 'Egresado' | 'Graduado';  anio?: number;             // año de graduación o de egreso; vacío en quinto año

  // Solo empresas
  contactoNombres?: string;
  contactoApellidos?: string;
  cargoContacto?: string;
  sitioWeb?: string;
}

@Component({
  selector: 'app-usuarios-admin',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './usuarios-admin.html',
  styleUrl: './usuarios-admin.css'
})
export class UsuariosAdmin {

  // Se llenará con los datos que devuelva la API
  usuarios = signal<Usuario[]>([]);

  busqueda = signal('');
  filtroTipo = signal('');
  filtroEstado = signal('');

  filtrados = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    const tipo = this.filtroTipo();
    const estado = this.filtroEstado();

    return this.usuarios().filter(u =>
      (!texto ||
        u.nombre.toLowerCase().includes(texto) ||
        u.correo.toLowerCase().includes(texto)) &&
      (!tipo || u.tipo === tipo) &&
      (!estado || u.estado === estado)
    );
  });

  limpiarFiltros(): void {
    this.busqueda.set('');
    this.filtroTipo.set('');
    this.filtroEstado.set('');
  }

  // Por ahora se muestra en una ventana simple; después se puede cambiar por un modal.
  ver(u: Usuario): void {
    const l: string[] = [
      `Nombre: ${u.nombre}`,
      `Correo: ${u.correo}`,
      `Tipo: ${u.tipo}`,
      `Estado: ${u.estado}`,
      `Fecha de registro: ${u.fechaRegistro}`,
    ];

    if (u.telefono) l.push(`Teléfono: ${u.telefono}`);

    if (u.tipo === 'Alumno') {
      if (u.carnet) l.push(`Carnet: ${u.carnet}`);
      if (u.situacion) l.push(`Situación: ${u.situacion}`);
      if (u.anio) {
        l.push(`${u.situacion === 'Egresado' ? 'Año de egreso' : 'Año de graduación'}: ${u.anio}`);
      }
    }

    if (u.tipo === 'Empresa') {
      const contacto = `${u.contactoNombres ?? ''} ${u.contactoApellidos ?? ''}`.trim();
      if (contacto) l.push(`Persona de contacto: ${contacto}`);
      if (u.cargoContacto) l.push(`Cargo del contacto: ${u.cargoContacto}`);
      if (u.sitioWeb) l.push(`Sitio web: ${u.sitioWeb}`);
    }

    alert(l.join('\n'));
  }

 activar(u: Usuario): void { this.cambiarEstado(u, 'Activa'); }
  suspender(u: Usuario): void { this.cambiarEstado(u, 'Suspendida'); }
  rechazar(u: Usuario): void { this.cambiarEstado(u, 'Rechazada'); }
  
  private cambiarEstado(u: Usuario, estado: Usuario['estado']): void {
    this.usuarios.update(lista => lista.map(x => (x.id === u.id ? { ...x, estado } : x)));
  }
}