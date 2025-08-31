import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  if (req.url.startsWith(environment.apiUrl)) {
    req = req.clone({ withCredentials: true });
  }

return next(req).pipe(
  catchError((error: HttpErrorResponse) => {
    const url = req.url.toLowerCase();

    if (
      error.status === 401 &&
      !url.includes('/accountManagement/me') &&
      !url.includes('/accountManagement/current-user')
    ) {
      router.navigate(['/account/login'], {
        queryParams: { returnUrl: router.url },
      });
    }

    return throwError(() => error);
  })
);

};
