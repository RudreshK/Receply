import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AppointmentListItem } from '../../core/models';
import { FeaturesService } from '../../core/features.service';

const DAY_OPTIONS = [1, 2, 3] as const;

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.scss'
})
export class AppointmentsComponent implements OnInit {
  viewDate = signal(new Date());
  appointments = signal<AppointmentListItem[]>([]);
  loading = signal(true);

  readonly dayOptions = DAY_OPTIONS;

  // Per-row transient state, keyed by appointmentId.
  updatingId = signal<string | null>(null);
  statusError = signal<string | null>(null);
  followUpOpenId = signal<string | null>(null);
  followUpInstructions = signal('');
  followUpDays = signal<Set<number>>(new Set([1, 3]));
  followUpSubmitting = signal(false);
  followUpError = signal<string | null>(null);
  followUpSentIds = signal<Set<string>>(new Set());

  readonly monthLabel = computed(() =>
    this.viewDate().toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
  );

  readonly sorted = computed(() =>
    [...this.appointments()].sort((a, b) => a.startUtc.localeCompare(b.startUtc))
  );

  readonly followUpsEnabled = computed(() => this.features.isEnabled('AppointmentFollowUps'));

  constructor(private readonly api: ApiService, private readonly features: FeaturesService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    const view = this.viewDate();
    const from = new Date(view.getFullYear(), view.getMonth(), 1).toISOString();
    const to = new Date(view.getFullYear(), view.getMonth() + 1, 1).toISOString();
    this.api.listAppointments(from, to).subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  changeMonth(delta: number): void {
    const view = this.viewDate();
    this.viewDate.set(new Date(view.getFullYear(), view.getMonth() + delta, 1));
    this.load();
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });
  }

  /** Outcome (Completed/NoShow) only makes sense once the appointment's slot has actually passed. */
  canSetOutcome(appt: AppointmentListItem): boolean {
    return (appt.status === 'Booked' || appt.status === 'Rescheduled') && new Date(appt.endUtc).getTime() < Date.now();
  }

  markStatus(appt: AppointmentListItem, status: 'Completed' | 'NoShow'): void {
    this.statusError.set(null);
    this.updatingId.set(appt.appointmentId);
    this.api.updateAppointmentStatus(appt.appointmentId, status).subscribe({
      next: () => {
        this.appointments.update((list) =>
          list.map((a) => (a.appointmentId === appt.appointmentId ? { ...a, status } : a))
        );
        this.updatingId.set(null);
      },
      error: () => {
        this.statusError.set("Couldn't update that appointment's status. Try again.");
        this.updatingId.set(null);
      }
    });
  }

  toggleFollowUp(appt: AppointmentListItem): void {
    if (this.followUpOpenId() === appt.appointmentId) {
      this.followUpOpenId.set(null);
      return;
    }
    this.followUpError.set(null);
    this.followUpInstructions.set('');
    this.followUpDays.set(new Set([1, 3]));
    this.followUpOpenId.set(appt.appointmentId);
  }

  toggleDay(day: number): void {
    const days = new Set(this.followUpDays());
    if (days.has(day)) {
      days.delete(day);
    } else {
      days.add(day);
    }
    this.followUpDays.set(days);
  }

  submitFollowUp(appt: AppointmentListItem): void {
    const instructions = this.followUpInstructions().trim();
    const days = [...this.followUpDays()].sort();

    if (!instructions) {
      this.followUpError.set('Add the instructions you want the client to receive.');
      return;
    }
    if (days.length === 0) {
      this.followUpError.set('Pick at least one day to send it.');
      return;
    }

    this.followUpError.set(null);
    this.followUpSubmitting.set(true);
    this.api.addAppointmentFollowUp(appt.appointmentId, instructions, days).subscribe({
      next: () => {
        this.followUpSubmitting.set(false);
        this.followUpOpenId.set(null);
        this.followUpSentIds.update((ids) => new Set(ids).add(appt.appointmentId));
      },
      error: () => {
        this.followUpSubmitting.set(false);
        this.followUpError.set("Couldn't schedule that follow-up. Try again.");
      }
    });
  }
}
