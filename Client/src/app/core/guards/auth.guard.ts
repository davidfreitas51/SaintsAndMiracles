import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export const AuthGuard: CanActivateFn = () => {
  const http = inject(HttpClient);
  const router = inject(Router);

  return http
    .get(`${environment.apiUrl}accountManagement/me`, { withCredentials: true })
    .pipe(
      map(() => true), 
      catchError(() => {
        router.navigate(['/account/login'], {
          queryParams: { returnUrl: router.url },
        });
        return of(false);
      })
    );
};
