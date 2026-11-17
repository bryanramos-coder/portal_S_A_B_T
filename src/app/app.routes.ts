import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Registro } from './registro/registro';
import { PlantillaPanel } from './plantillas/plantilla-panel/plantilla-panel';
import { InicioAdmin } from './paginas/admin/inicio-admin/inicio-admin';
import { CuentasAdmin } from './paginas/admin/cuentas-admin/cuentas-admin';
import { OfertasAdmin } from './paginas/admin/ofertas-admin/ofertas-admin';
import { InicioEmpresa } from './paginas/empresa/inicio-empresa/inicio-empresa';
import { OfertasEmpresa } from './paginas/empresa/ofertas-empresa/ofertas-empresa';
import { PerfilAdmin } from './paginas/admin/perfil-admin/perfil-admin';
import { UsuariosAdmin } from './paginas/admin/usuarios-admin/usuarios-admin';
import { SeguimientoAdmin} from './paginas/admin/seguimiento-admin/seguimiento-admin';

export const routes: Routes = [
  { path: 'login', component: Login, title: 'Iniciar sesión' },
  { path: 'registro', component: Registro, title: 'Crear cuenta' },

  {
    path: 'admin',
    component: PlantillaPanel,
    data: {
      rol: 'Administrador',
      menu: [
        { texto: 'Inicio',  ruta: '/admin',         icono: 'images/inicio.png' },
        { texto: 'Mi perfil', ruta: '/admin/perfil', icono: 'images/perfil.png' },
        { texto: 'Usuarios', ruta: '/admin/usuarios', icono: 'images/usuarios.png' },
        { texto: 'Ofertas', ruta: '/admin/ofertas', icono: 'images/ofertas.png' },
        { texto: 'Seguimiento', ruta: '/admin/seguimiento', icono: 'images/seguimiento.png' },
        { texto: 'Cerrar', ruta: '/admin/login', icono: 'images/cerrar_sesion.png' },

      ],
    },
    children: [
      { path: '', component: InicioAdmin },
      { path: 'cuentas', component: CuentasAdmin },
      { path: 'ofertas', component: OfertasAdmin },
      { path: '', component:InicioAdmin},
      { path: 'perfil', component:PerfilAdmin},
      { path: 'usuarios', component: UsuariosAdmin },
      { path: 'seguimiento', component: SeguimientoAdmin },

    ],
  },
  {
    path: 'empresa',
    component: PlantillaPanel,
    data: {
      rol: 'Empresa',
      menu: [
        { texto: 'Inicio',      ruta: '/empresa',         icono: '🏠' },
        { texto: 'Mis ofertas', ruta: '/empresa/ofertas', icono: '💼' },
      ],
    },
    children: [
      { path: '', component: InicioEmpresa },
      { path: 'ofertas', component: OfertasEmpresa },
    ],
  },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' },
];