import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

function withAuth(req: Parameters<HttpInterceptorFn>[0], token: string | null) {
  return token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // The auth endpoints themselves (login, signup, refresh, logout) never get a Bearer token, and a
  // 401 from one of them means the credentials really are wrong - not that an access token expired -
  // so they're excluded from the refresh-and-retry dance below. The platform-admin console has its
  // own entirely separate session (see PlatformAdminAuthService/platformAdminInterceptor) and must
  // never be touched here.
  if (req.url.includes('/api/platform-admin')) {
    return next(req);
  }

  const isAuthEndpoint = req.url.includes('/api/auth/') || req.url.includes('/api/signup/');

  return next(withAuth(req, auth.token)).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      if (!auth.isLoggedIn()) {
        router.navigateByUrl('/login');
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        switchMap((result) => next(withAuth(req, result.token))),
        catchError((refreshError: unknown) => {
          // The refresh token is invalid, expired, or revoked - there's no way back without signing
          // in again.
          auth.logout();
          router.navigateByUrl('/login');
          return throwError(() => refreshError);
        })
      );
    })
  );
};
