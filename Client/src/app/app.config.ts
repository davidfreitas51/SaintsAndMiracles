import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { csrfInterceptor } from './core/interceptors/csrf.interceptor';
import { CsrfTokenService } from './core/services/csrf-token.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),

    provideHttpClient(
      withInterceptors([csrfInterceptor, authInterceptor, loadingInterceptor]),
    ),

    provideAppInitializer(() => inject(CsrfTokenService).initializeToken()),
  ],
};
