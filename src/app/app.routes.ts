import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Registro } from './registro/registro';
import { PlantillaPanel } from './plantillas/plantilla-panel/plantilla-panel';
//Admin
import { InicioAdmin } from './paginas/admin/inicio-admin/inicio-admin';
import { PerfilAdmin } from './paginas/admin/perfil-admin/perfil-admin';
import { UsuariosAdmin } from './paginas/admin/usuarios-admin/usuarios-admin';
import { OfertasAdmin } from './paginas/admin/ofertas-admin/ofertas-admin';
import { SeguimientoAdmin } from './paginas/admin/seguimiento-admin/seguimiento-admin';
//Empresa
import { InicioEmpresa } from './paginas/empresa/inicio-empresa/inicio-empresa';
import { PerfilEmpresa } from './paginas/empresa/perfil-empresa/perfil-empresa';
import { OfertasEmpresa } from './paginas/empresa/ofertas-empresa/ofertas-empresa';
import { Postulantes } from './paginas/empresa/postulantes/postulantes';
//Alumno
import { InicioAlumno } from './paginas/alumno/inicio-alumno/inicio-alumno';
import { PerfilAlumno } from './paginas/alumno/perfil-alumno/perfil-alumno';
import { OfertasAlumno } from './paginas/alumno/ofertas-alumno/ofertas-alumno';
import { MisPostulaciones } from './paginas/alumno/mis-postulaciones/mis-postulaciones';

export const routes: Routes = [
  { path: 'login', component: Login, title: 'Iniciar sesión' },
  { path: 'registro', component: Registro, title: 'Crear cuenta' },

  {
    path: 'admin',
    component: PlantillaPanel,
    data: {
      rol: 'Administrador',
      menu: [
        { texto: 'Inicio', ruta: '/admin', icono: 'images/inicio.png' },
        { texto: 'Mi perfil', ruta: '/admin/perfil', icono: 'images/perfil.png' },
        { texto: 'Usuarios', ruta: '/admin/usuarios', icono: 'images/usuarios.png' },
        { texto: 'Ofertas', ruta: '/admin/ofertas', icono: 'images/ofertas.png' },
        { texto: 'Seguimiento', ruta: '/admin/seguimiento', icono: 'images/seguimiento.png' },
        { texto: 'Cerrar', ruta: '/admin/login', icono: 'images/cerrar_sesion.png' },
      ],
    },
    children: [
      { path: '', component: InicioAdmin },
      { path: 'perfil', component: PerfilAdmin },
      { path: 'usuarios', component: UsuariosAdmin },
      { path: 'ofertas', component: OfertasAdmin },
      { path: 'seguimiento', component: SeguimientoAdmin },
    ],
  },
  {
    path: 'empresa',
    component: PlantillaPanel,
    data: {
      rol: 'Empresa',
      menu: [
        { texto: 'Inicio', ruta: '/empresa', icono: 'images/inicio.png' },
        { texto: 'Mi perfil', ruta: '/empresa/perfil', icono: 'images/perfil.png' },
        { texto: 'Mis ofertas', ruta: '/empresa/ofertas', icono: 'images/ofertas.png' },
        { texto: 'Postulantes', ruta: '/empresa/postulantes', icono: 'images/usuarios.png' },
        { texto: 'Cerrar', ruta: '/empresa/login', icono: 'images/cerrar_sesion.png' },
      ],
    },
    children: [
      { path: '', component: InicioEmpresa },
      { path: 'perfil', component: PerfilEmpresa },
      { path: 'ofertas', component: OfertasEmpresa },
      { path: 'postulantes', component: Postulantes },
    ],
  },
  {
    path: 'alumno',
    component: PlantillaPanel,
    data: {
      rol: 'alumno',
      menu: [
        { texto: 'Inicio', ruta: '/alumno', icono: 'images/inicio.png' },
        { texto: 'Mi perfil', ruta: '/alumno/perfil', icono: 'images/perfil.png' },
        { texto: 'Ver ofertas', ruta: '/alumno/ofertas', icono: 'images/ofertas.png' },
        { texto: 'Mis postulaciones', ruta: '/alumno/postulaciones', icono: 'images/seguimiento.png' },
        { texto: 'Cerrar', ruta: '/alumno/login', icono: 'images/cerrar_sesion.png' },
      ],
    },
    children: [
      { path: '', component: InicioAlumno },
      { path: 'perfil', component: PerfilAlumno },
      { path: 'ofertas', component: OfertasAlumno },
      { path: 'postulaciones', component: MisPostulaciones },
    ],
  },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' },
];