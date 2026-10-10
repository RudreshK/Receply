import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { STAFF_ROLES, StaffListItem } from '../../core/models';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff.component.html',
  styleUrl: './staff.component.scss'
})
export class StaffComponent implements OnInit {
  readonly roles = STAFF_ROLES;

  staff = signal<StaffListItem[]>([]);
  loading = signal(true);
  loadError = signal<string | null>(null);

  fullName = '';
  email = '';
  phoneNumber = '';
  role = 'FrontDesk';

  creating = signal(false);
  createError = signal<string | null>(null);
  togglingId = signal<string | null>(null);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.listStaff().subscribe({
      next: (staff) => {
        this.staff.set(staff);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loadError.set(
          err instanceof HttpErrorResponse && err.status === 403
            ? 'Only the business owner can manage staff.'
            : "Couldn't load staff."
        );
        this.loading.set(false);
      }
    });
  }

  create(): void {
    this.createError.set(null);
    if (!this.fullName.trim() || !this.phoneNumber.trim()) return;

    this.creating.set(true);
    this.api
      .createStaff({
        fullName: this.fullName.trim(),
        email: this.email.trim() || null,
        phoneNumber: this.phoneNumber.trim(),
        role: this.role
      })
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.fullName = '';
          this.email = '';
          this.phoneNumber = '';
          this.role = 'FrontDesk';
          this.load();
        },
        error: (err: unknown) => {
          this.creating.set(false);
          this.createError.set(this.extractError(err));
        }
      });
  }

  toggleActive(member: StaffListItem): void {
    this.togglingId.set(member.id);
    this.api.setStaffActive(member.id, !member.isActive).subscribe({
      next: () => {
        this.togglingId.set(null);
        this.load();
      },
      error: () => this.togglingId.set(null)
    });
  }

  private extractError(err: unknown): string {
    if (err instanceof HttpErrorResponse && typeof err.error?.error === 'string') {
      return err.error.error;
    }
    return "Couldn't add that staff member.";
  }
}
