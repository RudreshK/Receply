import { Component, DestroyRef, computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BrandingService } from '../../core/branding.service';
import { FeaturesService } from '../../core/features.service';
import { RealtimeService } from '../../core/realtime.service';
import { SubscriptionStatus } from '../../core/models';
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
  readonly navLinks: { path: string; label: string; ownerOnly?: boolean }[] = [
    { path: '/app/inbox', label: 'WhatsApp Inbox' },
    { path: '/app/dashboard', label: 'Dashboard' },
    { path: '/app/settings', label: 'Business Settings' },
    { path: '/app/business-profile', label: 'Business Profile' },
    { path: '/app/staff', label: 'Staff', ownerOnly: true },
    { path: '/app/working-hours', label: 'Working Hours' },
    { path: '/app/holidays', label: 'Holidays' },
    { path: '/app/appointments', label: 'Appointments' },
    { path: '/app/faq', label: 'FAQ' },
    { path: '/app/subscription', label: 'Subscription' }
  ];

  readonly sidebarCollapsed = signal(this.readStoredCollapsed());
  /** The inbox wants every pixel it can get for the chat panes, so the shell drops its usual content
   * padding for that one route instead of boxing it in like the settings-style pages. */
  readonly fullBleed = signal(false);

  readonly onSubscriptionPage = signal(false);
  readonly subscriptionStatus = signal<SubscriptionStatus | null>(null);

  /** Blocks the whole dashboard once a trial/subscription has lapsed - the Subscription page itself
   * stays reachable so there's always a way out. Fails open on a load error rather than locking out
   * a paying customer over a transient network hiccup. */
  readonly showPaywall = computed(() => {
    const status = this.subscriptionStatus();
    return !!status && !status.hasActiveAccess && !this.onSubscriptionPage();
  });

  constructor(
    readonly auth: AuthService,
    readonly theme: ThemeService,
    readonly branding: BrandingService,
    private readonly api: ApiService,
    private readonly realtime: RealtimeService,
    private readonly features: FeaturesService,
    private readonly router: Router,
    destroyRef: DestroyRef
  ) {
    this.fullBleed.set(this.router.url.startsWith('/app/inbox'));
    this.onSubscriptionPage.set(this.router.url.startsWith('/app/subscription'));
    this.refreshSubscriptionStatus();
    this.branding.refresh();

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd), takeUntilDestroyed(destroyRef))
      .subscribe(event => {
        this.fullBleed.set(event.urlAfterRedirects.startsWith('/app/inbox'));
        this.onSubscriptionPage.set(event.urlAfterRedirects.startsWith('/app/subscription'));
        this.refreshSubscriptionStatus();
      });
  }

  private refreshSubscriptionStatus(): void {
    this.api.getSubscription().subscribe({
      next: (status) => this.subscriptionStatus.set(status),
      error: () => { /* don't change the paywall state over a failed check */ }
    });
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
