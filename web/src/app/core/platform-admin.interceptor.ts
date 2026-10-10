import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { PlatformAdminAuthService } from './platform-admin-auth.service';

/** Attaches the platform-admin bearer only to /api/platform-admin/* calls, so it never leaks onto a
 * regular business dashboard request and vice versa - see authInterceptor, which explicitly skips
 * this same path prefix for the same reason. */
export const platformAdminInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.includes('/api/platform-admin')) {
    return next(req);
  }

  const auth = inject(PlatformAdminAuthService);
  const token = auth.token;
  return next(token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req);
};
