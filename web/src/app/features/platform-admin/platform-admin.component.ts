import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BUSINESS_TYPES, BusinessEnquiry, BusinessListItem, PlatformStats } from '../../core/models';
import { PlatformAdminApiService } from '../../core/platform-admin-api.service';
import { PlatformAdminAuthService } from '../../core/platform-admin-auth.service';

type Tab = 'businesses' | 'enquiries';

@Component({
  selector: 'app-platform-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './platform-admin.component.html',
  styleUrl: './platform-admin.component.scss'
})
export class PlatformAdminComponent implements OnInit {
  readonly businessTypes = BUSINESS_TYPES;

  tab = signal<Tab>('businesses');

  stats = signal<PlatformStats | null>(null);
  businesses = signal<BusinessListItem[]>([]);
  enquiries = signal<BusinessEnquiry[]>([]);
  loading = signal(true);

  // New-enquiry form
  businessName = '';
  businessType = 'Clinic';
  contactName = '';
  phoneNumber = '';
  email = '';
  notes = '';
  creatingEnquiry = signal(false);
  enquiryError = signal<string | null>(null);

  // Convert dialog state: the enquiry currently being converted, if any.
  convertingEnquiry = signal<BusinessEnquiry | null>(null);
  convertTimeZoneId = 'Asia/Kolkata';
  converting = signal(false);
  convertError = signal<string | null>(null);

  actioningId = signal<string | null>(null);

  constructor(
    private readonly api: PlatformAdminApiService,
    private readonly auth: PlatformAdminAuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  private loadAll(): void {
    this.loading.set(true);
    this.api.getStats().subscribe({ next: (s) => this.stats.set(s), error: () => {} });
    this.api.listBusinesses().subscribe({
      next: (b) => {
        this.businesses.set(b);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.api.listEnquiries().subscribe({ next: (e) => this.enquiries.set(e), error: () => {} });
  }

  setTab(tab: Tab): void {
    this.tab.set(tab);
  }

  daysLeftLabel(daysLeft: number | null): string {
    if (daysLeft === null) return '—';
    return daysLeft < 0 ? 'Expired' : `${daysLeft}d`;
  }

  createEnquiry(): void {
    this.enquiryError.set(null);
    if (!this.businessName.trim() || !this.contactName.trim() || !this.phoneNumber.trim()) return;

    this.creatingEnquiry.set(true);
    this.api
      .createEnquiry({
        businessName: this.businessName.trim(),
        businessType: this.businessType,
        contactName: this.contactName.trim(),
        phoneNumber: this.phoneNumber.trim(),
        email: this.email.trim() || null,
        notes: this.notes.trim() || null
      })
      .subscribe({
        next: () => {
          this.creatingEnquiry.set(false);
          this.businessName = '';
          this.contactName = '';
          this.phoneNumber = '';
          this.email = '';
          this.notes = '';
          this.loadAll();
        },
        error: (err: unknown) => {
          this.creatingEnquiry.set(false);
          this.enquiryError.set(this.extractError(err, "Couldn't log that enquiry."));
        }
      });
  }

  startConvert(enquiry: BusinessEnquiry): void {
    this.convertError.set(null);
    this.convertTimeZoneId = 'Asia/Kolkata';
    this.convertingEnquiry.set(enquiry);
  }

  cancelConvert(): void {
    this.convertingEnquiry.set(null);
  }

  confirmConvert(): void {
    const enquiry = this.convertingEnquiry();
    if (!enquiry || !this.convertTimeZoneId.trim()) return;

    this.converting.set(true);
    this.convertError.set(null);
    this.api.convertEnquiry(enquiry.id, this.convertTimeZoneId.trim()).subscribe({
      next: () => {
        this.converting.set(false);
        this.convertingEnquiry.set(null);
        this.loadAll();
      },
      error: (err: unknown) => {
        this.converting.set(false);
        this.convertError.set(this.extractError(err, "Couldn't create that business."));
      }
    });
  }

  dismissEnquiry(enquiry: BusinessEnquiry): void {
    this.actioningId.set(enquiry.id);
    this.api.dismissEnquiry(enquiry.id).subscribe({
      next: () => {
        this.actioningId.set(null);
        this.loadAll();
      },
      error: () => this.actioningId.set(null)
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/admin/login');
  }

  private extractError(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse && typeof err.error?.error === 'string') {
      return err.error.error;
    }
    return fallback;
  }
}
