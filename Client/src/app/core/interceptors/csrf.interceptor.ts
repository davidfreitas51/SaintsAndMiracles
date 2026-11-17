import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CsrfTokenService } from '../services/csrf-token.service';

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
  const csrf = inject(CsrfTokenService);
  const method = req.method.toUpperCase();

  const safeMethods = ['GET', 'HEAD', 'OPTIONS'];
  const isApiRequest = req.url.includes('/api/');
  const isTokenEndpoint = req.url.includes('/api/security/csrf-token');

  if (!isApiRequest || isTokenEndpoint || safeMethods.includes(method)) {
    return next(req);
  }

  const token = csrf.getToken();

  if (!token) {
    throw new Error('Token CSRF indisponível para requisição protegida.');
  }

  return next(
    req.clone({
      withCredentials: true,
      setHeaders: {
        'X-CSRF-TOKEN': token,
      },
    }),
  );
};
