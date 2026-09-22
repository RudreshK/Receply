import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { BUSINESS_TYPES, TenantSettings } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  readonly businessTypes = BUSINESS_TYPES;

  name = '';
  businessType = 'Other';
  timeZoneId = '';
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.getTenantSettings().subscribe({
      next: (settings: TenantSettings) => {
        this.name = settings.name;
        this.businessType = settings.businessType;
        this.timeZoneId = settings.timeZoneId;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save(): void {
    this.saving.set(true);
    this.saved.set(false);
    this.api.updateTenantSettings({ name: this.name, businessType: this.businessType, timeZoneId: this.timeZoneId }).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
      },
      error: () => this.saving.set(false)
    });
  }
}
