import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, finalize, shareReplay, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RequestOtpResponse {
  code: string | null;
}

export interface VerifyOtpResponse {
  token: string;
  refreshToken: string;
  tenantId: string;
  staffId: string;
  fullName: string;
  role: string;
  preferredLanguage: string | null;
}

const STORAGE_KEY = 'receply.session';

export interface StoredSession {
  token: string;
  refreshToken: string;
  tenantId: string;
  staffId: string;
  fullName: string;
  role: string;
  preferredLanguage: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  // localStorage (not sessionStorage) so a refresh token that's valid for 30 days actually keeps a
  // staff member signed in across browser restarts, not just within one tab's lifetime.
  private readonly session = signal<StoredSession | null>(this.readStoredSession());
  private refreshInFlight$: Observable<VerifyOtpResponse> | null = null;

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

  requestSignupOtp(phoneNumber: string, email: string): Observable<RequestOtpResponse> {
    return this.http.post<RequestOtpResponse>(`${environment.apiBaseUrl}/api/signup/request-otp`, { phoneNumber, email });
  }

  completeSignup(payload: {
    phoneNumber: string;
    code: string;
    businessName: string;
    businessType: string;
    timeZoneId: string;
    ownerFirstName: string;
    ownerLastName: string;
    ownerEmail: string;
  }): Observable<VerifyOtpResponse> {
    return this.http
      .post<VerifyOtpResponse>(`${environment.apiBaseUrl}/api/signup/complete`, payload)
      .pipe(tap((result) => this.storeSession(result)));
  }

  /** Exchanges the stored refresh token for a new access token. Concurrent callers (several requests
   * all hitting a 401 at once - see authInterceptor) share the one in-flight HTTP call instead of
   * each triggering their own refresh and racing to rotate the same refresh token. */
  refreshAccessToken(): Observable<VerifyOtpResponse> {
    const current = this.session();
    if (!current) {
      return throwError(() => new Error('No session to refresh.'));
    }

    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.http
        .post<VerifyOtpResponse>(`${environment.apiBaseUrl}/api/auth/refresh`, { refreshToken: current.refreshToken })
        .pipe(
          tap((result) => this.storeSession(result)),
          shareReplay(1),
          finalize(() => { this.refreshInFlight$ = null; })
        );
    }

    return this.refreshInFlight$;
  }

  /** Persists a preferred-language change the caller already saved server-side (see MeService). */
  setPreferredLanguage(languageCode: string | null): void {
    const current = this.session();
    if (!current) return;
    this.storeSession({ ...current, preferredLanguage: languageCode });
  }

  logout(): void {
    const current = this.session();
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Non-fatal: the in-memory session is still cleared.
    }

    if (current?.refreshToken) {
      // Best-effort server-side revoke - the local session is already gone either way, so a staff
      // member is signed out immediately regardless of whether this call succeeds.
      this.http
        .post(`${environment.apiBaseUrl}/api/auth/logout`, { refreshToken: current.refreshToken })
        .subscribe({ error: () => { /* already logged out locally; nothing more to do */ } });
    }
  }

  private storeSession(result: VerifyOtpResponse): void {
    const stored: StoredSession = {
      token: result.token,
      refreshToken: result.refreshToken,
      tenantId: result.tenantId,
      staffId: result.staffId,
      fullName: result.fullName,
      role: result.role,
      preferredLanguage: result.preferredLanguage
    };
    this.session.set(stored);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    } catch {
      // Storage can be unavailable (private mode) - the session still works for this tab via the signal.
    }
  }

  private readStoredSession(): StoredSession | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredSession) : null;
    } catch {
      return null;
    }
  }
}
