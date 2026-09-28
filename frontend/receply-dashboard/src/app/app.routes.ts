import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'inbox', canActivate: [authGuard], loadComponent: () => import('./inbox/inbox').then(m => m.Inbox) },
  { path: '', pathMatch: 'full', redirectTo: 'inbox' },
  { path: '**', redirectTo: 'inbox' },
];
