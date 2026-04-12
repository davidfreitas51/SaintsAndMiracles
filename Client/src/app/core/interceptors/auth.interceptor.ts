import {
  HttpContextToken,
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CsrfTokenService } from '../services/csrf-token.service';

const HAS_RETRIED_CSRF = new HttpContextToken<boolean>(() => false);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const csrfTokenService = inject(CsrfTokenService);

  const isApiRequest = req.url.startsWith(environment.apiUrl);
  const isSafeMethod = ['GET', 'HEAD', 'OPTIONS'].includes(
    req.method.toUpperCase(),
  );

  if (req.url.startsWith(environment.apiUrl)) {
    req = req.clone({ withCredentials: true });

    if (!isSafeMethod) {
      const csrfToken = csrfTokenService.getToken();
      if (csrfToken) {
        req = req.clone({ setHeaders: { 'X-CSRF-TOKEN': csrfToken } });
      }
    }
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const url = req.url.toLowerCase();
      const hasRetriedCsrf = req.context.get(HAS_RETRIED_CSRF);

      // API restarts/deploys can invalidate the in-memory CSRF token. Refresh once and retry.
      if (
        isApiRequest &&
        !isSafeMethod &&
        !hasRetriedCsrf &&
        error.status === 400 &&
        isCsrfError(error)
      ) {
        return from(csrfTokenService.refreshToken()).pipe(
          switchMap((newToken) => {
            if (!newToken) {
              return throwError(() => error);
            }

            const retryReq = req.clone({
              setHeaders: { 'X-CSRF-TOKEN': newToken },
              context: req.context.set(HAS_RETRIED_CSRF, true),
            });

            return next(retryReq);
          }),
          catchError(() => throwError(() => error)),
        );
      }

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

function isCsrfError(error: HttpErrorResponse): boolean {
  const message =
    typeof error.error === 'string'
      ? error.error
      : ((error.error?.message as string | undefined) ?? '');

  return message.toLowerCase().includes('csrf');
}
