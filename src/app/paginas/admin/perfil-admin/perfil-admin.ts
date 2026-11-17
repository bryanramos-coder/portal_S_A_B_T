import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  fotoPerfil, guardarFoto, quitarFoto, FOTO_POR_DEFECTO
} from '../../../compartido/barra-superior/barra-superior';

@Component({
  selector: 'app-perfil-admin',
  imports: [FormsModule],
  templateUrl: './perfil-admin.html',
  styleUrl: './perfil-admin.css'
})
export class PerfilAdmin {
  editando = false;
  guardado = false;

  datos = {
    correo: 'admin@ues.edu.sv',
    cargo: 'Administrador del sistema',
  };
  copia = { ...this.datos };

  clave = { actual: '', nueva: '', confirmar: '' };
  mensajeClave = '';
  claveConError = false;

  editar() {
    this.copia = { ...this.datos };
    this.editando = true;
    this.guardado = false;
  }

  cancelar() {
    this.editando = false;
  }

  guardar() {
    this.datos = { ...this.copia };
    this.editando = false;
    this.guardado = true;
  }

  cambiarClave() {
    this.claveConError = true;
    if (!this.clave.actual) {
      this.mensajeClave = 'Escribe tu contraseña actual.';
    } else if (this.clave.nueva.length < 8) {
      this.mensajeClave = 'La nueva contraseña debe tener al menos 8 caracteres.';
    } else if (this.clave.nueva !== this.clave.confirmar) {
      this.mensajeClave = 'Las contraseñas no coinciden.';
    } else {
      this.claveConError = false;
      this.mensajeClave = 'Contraseña actualizada.';
      this.clave = { actual: '', nueva: '', confirmar: '' };
    }
  }



    foto = fotoPerfil;
  errorFoto = '';

  get tieneFotoPropia() {
    return fotoPerfil() !== FOTO_POR_DEFECTO;
  }

  elegirFoto(evento: Event) {
    const entrada = evento.target as HTMLInputElement;
    const archivo = entrada.files?.[0];
    entrada.value = '';
    this.errorFoto = '';
    if (!archivo) return;

    if (!archivo.type.startsWith('image/')) {
      this.errorFoto = 'El archivo debe ser una imagen (JPG o PNG).';
      return;
    }
    if (archivo.size > 5 * 1024 * 1024) {
      this.errorFoto = 'La imagen no debe pasar de 5 MB.';
      return;
    }

    const lector = new FileReader();
    lector.onload = () => {
      const img = new Image();
      img.onload = () => {
        const lado = Math.min(img.width, img.height);
        const x = (img.width - lado) / 2;
        const y = (img.height - lado) / 2;
        const lienzo = document.createElement('canvas');
        lienzo.width = 256;
        lienzo.height = 256;
        lienzo.getContext('2d')!.drawImage(img, x, y, lado, lado, 0, 0, 256, 256);
        guardarFoto(lienzo.toDataURL('image/jpeg', 0.85));
      };
      img.src = lector.result as string;
    };
    lector.readAsDataURL(archivo);
  }

  eliminarFoto() {
    quitarFoto();
  }
}