import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Branch, DashboardStats } from '../../core/models';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss'
})
export class DashboardHomeComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  branches = signal<Branch[]>([]);
  loading = signal(true);
  selectedBranchId = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (branches) => this.branches.set(branches)
    });
    this.loadStats();
  }

  onBranchChange(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.loading.set(true);
    this.api.getDashboardStats(this.selectedBranchId || null).subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
