import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CsrfTokenService } from '../services/csrf-token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const csrfTokenService = inject(CsrfTokenService);

  if (req.url.startsWith(environment.apiUrl)) {
    req = req.clone({ withCredentials: true });

    if (!['GET', 'HEAD', 'OPTIONS'].includes(req.method.toUpperCase())) {
      const csrfToken = csrfTokenService.getToken();
      if (csrfToken) {
        req = req.clone({ setHeaders: { 'X-CSRF-TOKEN': csrfToken } });
      }
    }
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const url = req.url.toLowerCase();

      if (
        error.status === 401 &&
        !url.includes('/accountmanagement/me') &&
        !url.includes('/accountmanagement/current-user')
      ) {
        router.navigate(['/account/login'], {
          queryParams: { returnUrl: router.url },
        });
      }

      return throwError(() => error);
    }),
  );
};
