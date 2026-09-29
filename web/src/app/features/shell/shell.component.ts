import { Component, DestroyRef, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { FeaturesService } from '../../core/features.service';
import { RealtimeService } from '../../core/realtime.service';
import { ThemeService } from '../../core/theme.service';

const SIDEBAR_COLLAPSED_KEY = 'receply.sidebarCollapsed';

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

  readonly sidebarCollapsed = signal(this.readStoredCollapsed());
  /** The inbox wants every pixel it can get for the chat panes, so the shell drops its usual content
   * padding for that one route instead of boxing it in like the settings-style pages. */
  readonly fullBleed = signal(false);

  constructor(
    readonly auth: AuthService,
    readonly theme: ThemeService,
    private readonly realtime: RealtimeService,
    private readonly features: FeaturesService,
    private readonly router: Router,
    destroyRef: DestroyRef
  ) {
    this.fullBleed.set(this.router.url.startsWith('/app/inbox'));

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd), takeUntilDestroyed(destroyRef))
      .subscribe(event => this.fullBleed.set(event.urlAfterRedirects.startsWith('/app/inbox')));
  }

  toggleSidebar(): void {
    const next = !this.sidebarCollapsed();
    this.sidebarCollapsed.set(next);
    try {
      localStorage.setItem(SIDEBAR_COLLAPSED_KEY, next ? '1' : '0');
    } catch {
      // Non-fatal: the choice just won't be remembered next visit.
    }
  }

  async logout(): Promise<void> {
    await this.realtime.stop();
    this.features.reset();
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  private readStoredCollapsed(): boolean {
    try {
      return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === '1';
    } catch {
      return false;
    }
  }
}
