import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { BarraLateral, ItemMenu } from '../../compartido/barra-lateral/barra-lateral';
import { BarraSuperior } from '../../compartido/barra-superior/barra-superior';

@Component({
  selector: 'app-plantilla-panel',
  imports: [RouterOutlet, BarraLateral, BarraSuperior],
  templateUrl: './plantilla-panel.html',
  styleUrl: './plantilla-panel.css'
})
export class PlantillaPanel {
  private ruta = inject(ActivatedRoute);
  rol: string = this.ruta.snapshot.data['rol'];
  menu: ItemMenu[] = this.ruta.snapshot.data['menu'];
}