import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountsService } from '../services/accounts.service';

export const AuthGuard: CanActivateFn = (route, state) => {
  const accountsService = inject(AccountsService);
  const router = inject(Router);

  if (accountsService.isLoggedIn()) {
    return true;
  } else {
    router.navigate(['/account/login'], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }
};
