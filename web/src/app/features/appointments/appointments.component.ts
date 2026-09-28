import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { AppointmentListItem } from '../../core/models';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.scss'
})
export class AppointmentsComponent implements OnInit {
  viewDate = signal(new Date());
  appointments = signal<AppointmentListItem[]>([]);
  loading = signal(true);

  readonly monthLabel = computed(() =>
    this.viewDate().toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
  );

  readonly sorted = computed(() =>
    [...this.appointments()].sort((a, b) => a.startUtc.localeCompare(b.startUtc))
  );

  constructor(private readonly api: ApiService) {}

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
}
