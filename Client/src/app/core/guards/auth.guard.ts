import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserSessionService } from '../services/user-session.service';

export const AuthGuard: CanActivateFn = (route, state) => {
  const userSessionService = inject(UserSessionService);
  const router = inject(Router);

  if (userSessionService.isLoggedIn()) {
    return true;
  } else {
    router.navigate(['/account/login'], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }
};
