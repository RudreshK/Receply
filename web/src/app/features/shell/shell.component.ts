import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { FeaturesService } from '../../core/features.service';
import { RealtimeService } from '../../core/realtime.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  readonly navLinks = [
    { path: '/app/inbox', label: 'WhatsApp Inbox' },
    { path: '/app/dashboard', label: 'Dashboard' },
    { path: '/app/settings', label: 'Business Settings' },
    { path: '/app/working-hours', label: 'Working Hours' },
    { path: '/app/holidays', label: 'Holidays' },
    { path: '/app/appointments', label: 'Appointments' },
    { path: '/app/faq', label: 'FAQ' }
  ];

  constructor(
    readonly auth: AuthService,
    private readonly realtime: RealtimeService,
    private readonly features: FeaturesService,
    private readonly router: Router
  ) {}

  async logout(): Promise<void> {
    await this.realtime.stop();
    this.features.reset();
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
