import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly step = signal<'phone' | 'code'>('phone');
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected phoneNumber = '';
  protected code = '';

  protected sendCode(): void {
    const phone = this.phoneNumber.trim();
    if (!phone) return;

    this.busy.set(true);
    this.error.set(null);
    this.auth.requestOtp(phone).subscribe({
      next: () => {
        this.busy.set(false);
        this.step.set('code');
      },
      error: (err: HttpErrorResponse) => {
        this.busy.set(false);
        this.error.set(err.status === 404 || err.status === 500
          ? 'No active staff account found for that phone number.'
          : 'Could not send a code. Please try again.');
      },
    });
  }

  protected verify(): void {
    const code = this.code.trim();
    if (!code) return;

    this.busy.set(true);
    this.error.set(null);
    this.auth.verifyOtp(this.phoneNumber.trim(), code).subscribe({
      next: () => this.router.navigate(['/inbox']),
      error: () => {
        this.busy.set(false);
        this.error.set('That code is invalid or has expired.');
      },
    });
  }

  protected back(): void {
    this.step.set('phone');
    this.code = '';
    this.error.set(null);
  }
}
