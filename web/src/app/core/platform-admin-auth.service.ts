import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PlatformAdminRequestOtpResponse {
  code: string | null;
}

export interface PlatformAdminLoginResult {
  token: string;
  platformAdminId: string;
  fullName: string;
}

const STORAGE_KEY = 'receply.platformAdminSession';

export interface StoredPlatformAdminSession {
  token: string;
  platformAdminId: string;
  fullName: string;
}

/** Entirely separate from AuthService/StoredSession - a platform admin isn't a Staff member and
 * carries no tenant_id, so it gets its own storage key and its own login endpoints instead of
 * sharing the business dashboard's session. */
@Injectable({ providedIn: 'root' })
export class PlatformAdminAuthService {
  private readonly session = signal<StoredPlatformAdminSession | null>(this.readStoredSession());

  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly currentAdmin = computed(() => this.session());

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  requestOtp(phoneNumber: string): Observable<PlatformAdminRequestOtpResponse> {
    return this.http.post<PlatformAdminRequestOtpResponse>(`${environment.apiBaseUrl}/api/platform-admin/auth/request-otp`, { phoneNumber });
  }

  verifyOtp(phoneNumber: string, code: string): Observable<PlatformAdminLoginResult> {
    return this.http
      .post<PlatformAdminLoginResult>(`${environment.apiBaseUrl}/api/platform-admin/auth/verify-otp`, { phoneNumber, code })
      .pipe(tap((result) => this.storeSession(result)));
  }

  logout(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Non-fatal: the in-memory session is still cleared.
    }
  }

  private storeSession(result: PlatformAdminLoginResult): void {
    const stored: StoredPlatformAdminSession = {
      token: result.token,
      platformAdminId: result.platformAdminId,
      fullName: result.fullName
    };
    this.session.set(stored);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    } catch {
      // Storage can be unavailable (private mode) - the session still works for this tab via the signal.
    }
  }

  private readStoredSession(): StoredPlatformAdminSession | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredPlatformAdminSession) : null;
    } catch {
      return null;
    }
  }
}
