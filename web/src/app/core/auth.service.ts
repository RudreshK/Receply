import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RequestOtpResponse {
  code: string | null;
}

export interface VerifyOtpResponse {
  token: string;
  tenantId: string;
  staffId: string;
  fullName: string;
  role: string;
}

const STORAGE_KEY = 'receply.session';

interface StoredSession {
  token: string;
  tenantId: string;
  staffId: string;
  fullName: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly currentUser = computed(() => this.session());

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  requestOtp(phoneNumber: string): Observable<RequestOtpResponse> {
    return this.http.post<RequestOtpResponse>(`${environment.apiBaseUrl}/api/auth/request-otp`, { phoneNumber });
  }

  verifyOtp(phoneNumber: string, code: string): Observable<VerifyOtpResponse> {
    return this.http
      .post<VerifyOtpResponse>(`${environment.apiBaseUrl}/api/auth/verify-otp`, { phoneNumber, code })
      .pipe(tap((result) => this.storeSession(result)));
  }

  logout(): void {
    this.session.set(null);
    sessionStorage.removeItem(STORAGE_KEY);
  }

  private storeSession(result: VerifyOtpResponse): void {
    const stored: StoredSession = {
      token: result.token,
      tenantId: result.tenantId,
      staffId: result.staffId,
      fullName: result.fullName,
      role: result.role
    };
    this.session.set(stored);
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    } catch {
      // sessionStorage unavailable (private browsing, etc.) - session still works for this tab via the signal.
    }
  }

  private readStoredSession(): StoredSession | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredSession) : null;
    } catch {
      return null;
    }
  }
}
