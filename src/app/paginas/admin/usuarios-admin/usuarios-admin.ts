import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface Usuario {
  id: number;
  nombre: string;
  correo: string;

  tipo: 'Alumno' | 'Empresa' | 'Administrador';

  estado:
    | 'Pendiente'
    | 'Habilitado'
    | 'Deshabilitado'
    | 'Rechazado';

  fechaRegistro: string;

  // Solo egresados / estudiantes
  carnet: string;
  situacion:
    | 'Graduado'
    | 'Egresado'
    | 'Estudiante de quinto año'
    | 'En trámite de graduación';

  anioGraduacion?: number;

  // Solo empresas
  telefono: string;
  personaContacto: string;
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

  usuarios = signal<Usuario[]>([]);

  busqueda = signal('');
  filtroTipo = signal('');
  filtroEstado = signal('');

  filtrados = computed(() => {

    const texto = this.busqueda()
      .trim()
      .toLowerCase();

    const tipo = this.filtroTipo();
    const estado = this.filtroEstado();

    return this.usuarios().filter(usuario => {

      const coincideBusqueda =
        !texto ||
        usuario.nombre.toLowerCase().includes(texto) ||
        usuario.correo.toLowerCase().includes(texto);

      const coincideTipo =
        !tipo || usuario.tipo === tipo;

      const coincideEstado =
        !estado || usuario.estado === estado;

      return (
        coincideBusqueda &&
        coincideTipo &&
        coincideEstado
      );
    });
  });

  limpiarFiltros(): void {
    this.busqueda.set('');
    this.filtroTipo.set('');
    this.filtroEstado.set('');
  }

  /**
   * Mostrar información del usuario.
   * Por ahora se muestra en una ventana simple.
   * Después podemos sustituirlo por un modal.
   */
  ver(usuario: Usuario): void {

    let informacion = `
Nombre: ${usuario.nombre}
Correo: ${usuario.correo}
Tipo: ${usuario.tipo}
Estado: ${usuario.estado}
Fecha de registro: ${usuario.fechaRegistro}
`;

    if (usuario.carnet) {
      informacion += `Carnet: ${usuario.carnet}\n`;
    }

    if (usuario.situacion) {
      informacion += `Situación: ${usuario.situacion}\n`;
    }

    if (usuario.anioGraduacion) {
      informacion += `Año de graduación: ${usuario.anioGraduacion}\n`;
    }

    if (usuario.telefono) {
      informacion += `Teléfono: ${usuario.telefono}\n`;
    }

    if (usuario.personaContacto) {
      informacion += `Persona de contacto: ${usuario.personaContacto}\n`;
    }

    if (usuario.sitioWeb) {
      informacion += `Sitio web: ${usuario.sitioWeb}\n`;
    }

    alert(informacion);
  }

  /**
   * Habilitar usuario
   */
  habilitar(usuario: Usuario): void {

    this.usuarios.update(lista =>
      lista.map(u =>
        u.id === usuario.id
          ? { ...u, estado: 'Habilitado' }
          : u
      )
    );
  }

  /**
   * Deshabilitar usuario
   */
  deshabilitar(usuario: Usuario): void {

    this.usuarios.update(lista =>
      lista.map(u =>
        u.id === usuario.id
          ? { ...u, estado: 'Deshabilitado' }
          : u
      )
    );
  }

  /**
   * Rechazar usuario
   */
  rechazar(usuario: Usuario): void {

    this.usuarios.update(lista =>
      lista.map(u =>
        u.id === usuario.id
          ? { ...u, estado: 'Rechazado' }
          : u
      )
    );
  }
}