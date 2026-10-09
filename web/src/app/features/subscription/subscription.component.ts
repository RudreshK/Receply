import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { PlanOption, SubscriptionStatus } from '../../core/models';

const FEATURE_LABELS: Record<string, string> = {
  ManualStaffReply: 'Staff can reply by hand and take chats back from the AI',
  ReadReceipts: 'WhatsApp read receipts and "typing…" indicator',
  AppointmentBooking: 'AI can book, reschedule, and cancel appointments',
  MessageTranslation: 'On-demand message translation',
  AppointmentReminders: 'Automatic appointment reminders',
  AppointmentOutcomeMessages: 'Automatic completed / missed-appointment messages',
  AppointmentFollowUps: 'Post-appointment care follow-ups',
  PerformanceDashboard: 'Performance dashboard (new-client counts, etc.)'
};

const PLAN_ORDER = ['Trial', 'Starter', 'Growth', 'AssistedSetup'];

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription.component.html',
  styleUrl: './subscription.component.scss'
})
export class SubscriptionComponent implements OnInit {
  loading = signal(true);
  status = signal<SubscriptionStatus | null>(null);
  switching = signal<string | null>(null);
  error = signal<string | null>(null);

  readonly isOwner = computed(() => this.auth.currentUser()?.role === 'Owner');

  readonly orderedPlans = computed(() => {
    const plans = this.status()?.availablePlans ?? [];
    return [...plans].sort((a, b) => PLAN_ORDER.indexOf(a.plan) - PLAN_ORDER.indexOf(b.plan));
  });

  readonly currentPlanDisplayName = computed(() => {
    const s = this.status();
    return this.orderedPlans().find((p) => p.plan === s?.plan)?.displayName ?? s?.plan ?? '';
  });

  readonly trialDaysLeft = computed(() => {
    const endsUtc = this.status()?.trialEndsUtc;
    if (!endsUtc) return null;
    const ms = new Date(endsUtc).getTime() - Date.now();
    return Math.max(0, Math.ceil(ms / (1000 * 60 * 60 * 24)));
  });

  constructor(private readonly api: ApiService, private readonly auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  featureLabel(key: string): string {
    return FEATURE_LABELS[key] ?? key;
  }

  isCurrentPlan(plan: PlanOption): boolean {
    return this.status()?.plan === plan.plan;
  }

  switchTo(plan: PlanOption): void {
    if (plan.plan === 'Trial' || this.isCurrentPlan(plan)) return;

    this.error.set(null);
    this.switching.set(plan.plan);
    this.api.changeSubscriptionPlan(plan.plan).subscribe({
      next: () => this.load(() => this.switching.set(null)),
      error: () => {
        this.error.set("Couldn't switch plans. Try again.");
        this.switching.set(null);
      }
    });
  }

  private load(onDone?: () => void): void {
    this.loading.set(true);
    this.api.getSubscription().subscribe({
      next: (status) => {
        this.status.set(status);
        this.loading.set(false);
        onDone?.();
      },
      error: () => {
        this.loading.set(false);
        onDone?.();
      }
    });
  }
}
