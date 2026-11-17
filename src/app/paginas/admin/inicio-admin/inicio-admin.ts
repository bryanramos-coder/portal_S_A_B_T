import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-inicio-admin',
  imports: [RouterLink],
  templateUrl: './inicio-admin.html',
  styleUrl: './inicio-admin.css'
})
export class InicioAdmin {
  hoy = new Date().toLocaleDateString('es-SV', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
  });

  indicadores = [
    { titulo: 'Egresados registrados',     color: '#3b82f6' },
    { titulo: 'Empresas activas',          color: '#22c55e' },
    { titulo: 'Ofertas publicadas',        color: '#0bf5bb' },
    { titulo: 'Postulaciones',             color: '#f75583' },
  ];
}