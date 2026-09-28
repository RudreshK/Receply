import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Session {
  token: string;
  tenantId: string;
  staffId: string;
  fullName: string;
  role: string;
  preferredLanguage: string | null;
}

const STORAGE_KEY = 'receply.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly sessionSignal = signal<Session | null>(this.readStoredSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.sessionSignal() !== null);

  requestOtp(phoneNumber: string): Observable<unknown> {
    return this.http.post(`${environment.apiBaseUrl}/api/auth/request-otp`, { phoneNumber });
  }

  verifyOtp(phoneNumber: string, code: string): Observable<Session> {
    return this.http
      .post<Session>(`${environment.apiBaseUrl}/api/auth/verify-otp`, { phoneNumber, code })
      .pipe(tap(session => this.store(session)));
  }

  /** Persists a preferred-language change the caller already saved server-side (see MeService). */
  setPreferredLanguage(languageCode: string | null): void {
    const current = this.sessionSignal();
    if (!current) return;
    this.store({ ...current, preferredLanguage: languageCode });
  }

  logout(): void {
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Storage can be unavailable (private mode); the in-memory session is still cleared.
    }
    this.sessionSignal.set(null);
  }

  private store(session: Session): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      // Non-fatal: the session still lives in memory for this tab.
    }
    this.sessionSignal.set(session);
  }

  private readStoredSession(): Session | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const session = JSON.parse(raw) as Session;
      return this.isExpired(session.token) ? null : session;
    } catch {
      return null;
    }
  }

  private isExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return typeof payload.exp === 'number' && payload.exp * 1000 <= Date.now();
    } catch {
      return true;
    }
  }
}
