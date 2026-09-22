import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  readonly navLinks = [
    { path: '/app/dashboard', label: 'Dashboard' },
    { path: '/app/settings', label: 'Business Settings' },
    { path: '/app/working-hours', label: 'Working Hours' },
    { path: '/app/holidays', label: 'Holidays' },
    { path: '/app/appointments', label: 'Appointments' },
    { path: '/app/faq', label: 'FAQ' }
  ];

  constructor(readonly auth: AuthService, private readonly router: Router) {}

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
