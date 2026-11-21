import { Component } from '@angular/core';
import { datosEmpresa } from '../perfil-empresa/perfil-empresa';

@Component({
  selector: 'app-inicio-empresa',
  imports: [],
  templateUrl: './inicio-empresa.html',
  styleUrl: './inicio-empresa.css'
})
export class InicioEmpresa {
  empresa = datosEmpresa;

  fecha = new Date().toLocaleDateString('es-SV', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
  });
}