import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Branch, BranchWorkingHours, DAYS_OF_WEEK } from '../../core/models';

@Component({
  selector: 'app-working-hours',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './working-hours.component.html',
  styleUrl: './working-hours.component.scss'
})
export class WorkingHoursComponent implements OnInit {
  branches = signal<Branch[]>([]);
  selectedBranchId = signal<string | null>(null);
  hours = signal<BranchWorkingHours[]>([]);
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (branches) => {
        this.branches.set(branches);
        if (branches.length > 0) {
          this.selectBranch(branches[0].id);
        } else {
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  selectBranch(branchId: string): void {
    this.selectedBranchId.set(branchId);
    this.loading.set(true);
    this.saved.set(false);
    this.api.getWorkingHours(branchId).subscribe({
      next: (hours) => {
        this.hours.set(this.mergeWithFullWeek(hours));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private mergeWithFullWeek(existing: BranchWorkingHours[]): BranchWorkingHours[] {
    return DAYS_OF_WEEK.map((day) => {
      const found = existing.find((h) => h.dayOfWeek === day);
      return found ?? { dayOfWeek: day, openTime: '09:00:00', closeTime: '18:00:00', isClosed: day === 'Sunday' };
    });
  }

  save(): void {
    const branchId = this.selectedBranchId();
    if (!branchId) {
      return;
    }
    this.saving.set(true);
    this.saved.set(false);
    this.api.setWorkingHours(branchId, this.hours()).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
      },
      error: () => this.saving.set(false)
    });
  }
}
