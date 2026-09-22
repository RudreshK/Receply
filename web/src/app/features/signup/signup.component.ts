import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { BUSINESS_TYPES } from '../../core/models';

type Step = 'details' | 'code';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.scss'
})
export class SignupComponent {
  readonly businessTypes = BUSINESS_TYPES;

  step = signal<Step>('details');
  loading = signal(false);
  error = signal<string | null>(null);
  devCode = signal<string | null>(null);

  businessName = '';
  businessType = 'Other';
  timeZoneId = Intl.DateTimeFormat().resolvedOptions().timeZone || '';
  ownerFullName = '';
  ownerPhoneNumber = '';
  ownerEmail = '';
  code = '';

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  requestOtp(): void {
    if (!this.businessName.trim() || !this.ownerFullName.trim() || !this.ownerPhoneNumber.trim() || !this.timeZoneId.trim()) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth.requestSignupOtp(this.ownerPhoneNumber.trim()).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.devCode.set(result.code);
        this.step.set('code');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('That phone number is already registered - try signing in instead.');
      }
    });
  }

  completeSignup(): void {
    if (!this.code.trim()) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth
      .completeSignup({
        phoneNumber: this.ownerPhoneNumber.trim(),
        code: this.code.trim(),
        businessName: this.businessName.trim(),
        businessType: this.businessType,
        timeZoneId: this.timeZoneId.trim(),
        ownerFullName: this.ownerFullName.trim(),
        ownerEmail: this.ownerEmail.trim() || null
      })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigateByUrl('/app/dashboard');
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Incorrect or expired code.');
        }
      });
  }

  backToDetails(): void {
    this.step.set('details');
    this.code = '';
    this.error.set(null);
  }
}
