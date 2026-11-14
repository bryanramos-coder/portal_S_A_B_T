import { Component, ElementRef, HostListener, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
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
