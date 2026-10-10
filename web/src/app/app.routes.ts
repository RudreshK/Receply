import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { platformAdminGuard } from './core/platform-admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component').then((m) => m.LandingComponent)
  },
  { path: 'login', loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent) },
  { path: 'signup', loadComponent: () => import('./features/signup/signup.component').then((m) => m.SignupComponent) },
  // Platform-owner console - deliberately not linked from anywhere a business owner/staff member
  // can see (no nav entry in ShellComponent), and gated by its own guard/session, not authGuard.
  {
    path: 'admin/login',
    loadComponent: () =>
      import('./features/platform-admin-login/platform-admin-login.component').then((m) => m.PlatformAdminLoginComponent)
  },
  {
    path: 'admin',
    canActivate: [platformAdminGuard],
    loadComponent: () => import('./features/platform-admin/platform-admin.component').then((m) => m.PlatformAdminComponent)
  },
  {
    path: 'app',
    loadComponent: () => import('./features/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'inbox' },
      {
        path: 'inbox',
        loadComponent: () => import('./features/inbox/inbox.component').then((m) => m.InboxComponent)
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard-home/dashboard-home.component').then((m) => m.DashboardHomeComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent)
      },
      {
        path: 'business-profile',
        loadComponent: () =>
          import('./features/business-profile/business-profile.component').then((m) => m.BusinessProfileComponent)
      },
      {
        path: 'staff',
        loadComponent: () => import('./features/staff/staff.component').then((m) => m.StaffComponent)
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
      { path: 'faq', loadComponent: () => import('./features/faq/faq.component').then((m) => m.FaqComponent) },
      {
        path: 'subscription',
        loadComponent: () => import('./features/subscription/subscription.component').then((m) => m.SubscriptionComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
