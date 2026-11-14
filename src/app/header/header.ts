import { Component, ElementRef, HostListener, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  template: `
    <header>
      <div class="logos">
        <img src="/images/logo_fmo1.png" alt="Facultad Multidisciplinaria Oriental" class="logoTipo-img">
        <img src="/images/logotipo_ues.png" alt="Universidad de El Salvador" class="logoTipo-img">
      </div>
      <div class="portal-title">Portal Bolsa de Trabajo FMO</div>
      <div class="header-controls">
        <select class="language-select" aria-label="Idioma">
          <option value="es">Español</option>
          <option value="en">English</option>
        </select>
        <div class="menu-container">
          <button class="menu-btn" aria-label="Menú de opciones" title="Menú"
                  [attr.aria-expanded]="abierto()" (click)="alternar($event)">☰</button>
          <div class="dropdown-content" [class.show]="abierto()">
            <ul>
              @for (e of enlaces(); track e.ruta) {
                <li><a [routerLink]="e.ruta" (click)="abierto.set(false)">{{ e.texto }}</a></li>
              }
            </ul>
          </div>
        </div>
      </div>
    </header>
  `,
})
export class Header {
  enlaces = input<{ texto: string; ruta: string }[]>([]);
  abierto = signal(false);
  private el = inject(ElementRef);

  alternar(e: Event) {
    e.stopPropagation();
    this.abierto.update(v => !v);
  }

  @HostListener('document:click', ['$event'])
  clicFuera(e: Event) {
    if (!this.el.nativeElement.contains(e.target)) this.abierto.set(false);
  }

  @HostListener('document:keydown.escape')
  escape() {
    this.abierto.set(false);
  }
}
