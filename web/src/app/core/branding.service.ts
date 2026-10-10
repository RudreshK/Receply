import { Injectable, signal } from '@angular/core';
import { ApiService } from './api.service';
import { BusinessBranding } from './models';

/** Shared across the shell sidebar, dashboard header and the Business Profile page, so saving a
 * change in one place shows up everywhere else without a reload. */
@Injectable({ providedIn: 'root' })
export class BrandingService {
  readonly branding = signal<BusinessBranding | null>(null);

  constructor(private readonly api: ApiService) {}

  refresh(): void {
    this.api.getBranding().subscribe({
      next: (branding) => this.branding.set(branding),
      error: () => { /* keep whatever was last loaded rather than falling back on a blip */ }
    });
  }
}
