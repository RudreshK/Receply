import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PlatformAdminAuthService } from './platform-admin-auth.service';

export const platformAdminGuard: CanActivateFn = () => {
  const auth = inject(PlatformAdminAuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/admin/login']);
};
