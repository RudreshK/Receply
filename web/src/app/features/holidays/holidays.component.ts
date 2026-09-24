import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Holiday } from '../../core/models';

interface CalendarCell {
  date: string | null;
  day: number | null;
  holiday: Holiday | null;
}

@Component({
  selector: 'app-holidays',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './holidays.component.html',
  styleUrl: './holidays.component.scss'
})
export class HolidaysComponent implements OnInit {
  viewDate = signal(new Date());
  holidays = signal<Holiday[]>([]);
  loading = signal(true);

  newDate = '';
  newName = '';

  readonly monthLabel = computed(() =>
    this.viewDate().toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
  );

  readonly cells = computed<CalendarCell[]>(() => {
    const view = this.viewDate();
    const year = view.getFullYear();
    const month = view.getMonth();
    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const holidayByDate = new Map(this.holidays().map((h) => [h.date, h]));

    const cells: CalendarCell[] = [];
    for (let i = 0; i < firstDay; i++) {
      cells.push({ date: null, day: null, holiday: null });
    }
    for (let day = 1; day <= daysInMonth; day++) {
      const date = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
      cells.push({ date, day, holiday: holidayByDate.get(date) ?? null });
    }
    return cells;
  });

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadHolidays();
  }

  private loadHolidays(): void {
    this.loading.set(true);
    const view = this.viewDate();
    const from = `${view.getFullYear()}-01-01`;
    const to = `${view.getFullYear()}-12-31`;
    this.api.listHolidays(from, to).subscribe({
      next: (holidays) => {
        this.holidays.set(holidays);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  changeMonth(delta: number): void {
    const view = this.viewDate();
    this.viewDate.set(new Date(view.getFullYear(), view.getMonth() + delta, 1));
  }

  addHoliday(): void {
    if (!this.newDate || !this.newName.trim()) {
      return;
    }
    this.api.createHoliday({ branchId: null, date: this.newDate, name: this.newName.trim() }).subscribe(() => {
      this.newDate = '';
      this.newName = '';
      this.loadHolidays();
    });
  }

  removeHoliday(id: string): void {
    this.api.deleteHoliday(id).subscribe(() => this.loadHolidays());
  }
}
