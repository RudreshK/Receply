import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

/** Mirrors the Feature enum in Receply.Domain.Tenancy - the plug-and-play, per-tenant, subscription-gated capabilities. */
export type FeatureKey =
  | 'ManualStaffReply'
  | 'ReadReceipts'
  | 'MessageTranslation'
  | 'AppointmentReminders'
  | 'AppointmentOutcomeMessages'
  | 'AppointmentFollowUps'
  | 'AppointmentBooking'
  | 'PerformanceDashboard';

@Injectable({ providedIn: 'root' })
export class FeaturesService {
  private readonly http = inject(HttpClient);
  private readonly flags = signal<Partial<Record<FeatureKey, boolean>>>({});
  private loaded = false;

  readonly features = this.flags.asReadonly();

  /** Loads the tenant's feature flags once per session; subsequent calls are a no-op until reset(). */
  async load(): Promise<void> {
    if (this.loaded) return;
    try {
      const result = await firstValueFrom(this.http.get<Partial<Record<FeatureKey, boolean>>>(`${environment.apiBaseUrl}/api/features`));
      this.flags.set(result);
      this.loaded = true;
    } catch {
      // A load failure shouldn't hide features that are actually enabled - the backend still
      // enforces each one at its own endpoint regardless of what the dashboard shows.
      this.flags.set({});
    }
  }

  isEnabled(feature: FeatureKey): boolean {
    return this.flags()[feature] ?? true;
  }

  reset(): void {
    this.loaded = false;
    this.flags.set({});
  }
}
