import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { DashboardStats } from '../../core/models';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss'
})
export class DashboardHomeComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  loading = signal(true);
  planLocked = signal(false);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.getDashboardStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.planLocked.set(err instanceof HttpErrorResponse && err.status === 403);
        this.loading.set(false);
      }
    });
  }
}
