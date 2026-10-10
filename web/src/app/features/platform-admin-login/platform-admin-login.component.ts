import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PlatformAdminAuthService } from '../../core/platform-admin-auth.service';

type Step = 'phone' | 'code';

@Component({
  selector: 'app-platform-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './platform-admin-login.component.html',
  styleUrl: './platform-admin-login.component.scss'
})
export class PlatformAdminLoginComponent {
  step = signal<Step>('phone');
  phoneNumber = '';
  code = '';
  loading = signal(false);
  error = signal<string | null>(null);
  devCode = signal<string | null>(null);

  constructor(private readonly auth: PlatformAdminAuthService, private readonly router: Router) {}

  requestOtp(): void {
    if (!this.phoneNumber.trim()) return;

    this.loading.set(true);
    this.error.set(null);
    this.auth.requestOtp(this.phoneNumber.trim()).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.devCode.set(result.code);
        this.step.set('code');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('No admin account found for that phone number.');
      }
    });
  }

  verifyOtp(): void {
    if (!this.code.trim()) return;

    this.loading.set(true);
    this.error.set(null);
    this.auth.verifyOtp(this.phoneNumber.trim(), this.code.trim()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/admin');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Incorrect or expired code.');
      }
    });
  }

  backToPhone(): void {
    this.step.set('phone');
    this.code = '';
    this.error.set(null);
  }
}
