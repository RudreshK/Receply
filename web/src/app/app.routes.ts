import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component').then((m) => m.LandingComponent)
  },
  { path: 'login', loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent) },
  {
    path: 'app',
    loadComponent: () => import('./features/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard-home/dashboard-home.component').then((m) => m.DashboardHomeComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent)
      },
      {
        path: 'working-hours',
        loadComponent: () => import('./features/working-hours/working-hours.component').then((m) => m.WorkingHoursComponent)
      },
      {
        path: 'holidays',
        loadComponent: () => import('./features/holidays/holidays.component').then((m) => m.HolidaysComponent)
      },
      {
        path: 'appointments',
        loadComponent: () => import('./features/appointments/appointments.component').then((m) => m.AppointmentsComponent)
      },
      { path: 'faq', loadComponent: () => import('./features/faq/faq.component').then((m) => m.FaqComponent) }
    ]
  },
  { path: '**', redirectTo: '' }
];
