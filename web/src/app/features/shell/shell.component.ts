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
    { path: '/dashboard', label: 'Dashboard' },
    { path: '/settings', label: 'Business Settings' },
    { path: '/working-hours', label: 'Working Hours' },
    { path: '/holidays', label: 'Holidays' },
    { path: '/appointments', label: 'Appointments' },
    { path: '/faq', label: 'FAQ' }
  ];

  constructor(readonly auth: AuthService, private readonly router: Router) {}

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
