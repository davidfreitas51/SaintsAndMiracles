import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient,
  withInterceptors,
  HttpClient,
} from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { Router } from '@angular/router';
import { createRouterSpy } from '../../../testing/router-spy';
import { environment } from '../../../environments/environment';
import { CsrfTokenService } from '../services/csrf-token.service';

class CsrfTokenServiceStub {
  private token: string | null = null;

  public getToken(): string | null {
    return this.token;
  }

  public async initializeToken(): Promise<void> {
    // no-op in tests
  }

  public async refreshToken(): Promise<string | null> {
    this.token = 'refreshed-csrf-token';
    return this.token;
  }

  public setToken(token: string | null): void {
    this.token = token;
  }
}

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: ReturnType<typeof createRouterSpy>;
  let csrfTokenService: CsrfTokenServiceStub;

  beforeEach(() => {
    router = createRouterSpy('/admin');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
        { provide: CsrfTokenService, useClass: CsrfTokenServiceStub },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    csrfTokenService = TestBed.inject(
      CsrfTokenService,
    ) as unknown as CsrfTokenServiceStub;
  });

  afterEach(() => httpMock.verify());

  it('adds credentials for API calls', () => {
    http.get(`${environment.apiUrl}saints`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(req.request.withCredentials).toBeTrue();
    req.flush([]);
  });

  it('adds csrf header for mutating API calls when token exists', () => {
    csrfTokenService.setToken('initial-csrf-token');

    http.post(`${environment.apiUrl}saints`, { name: 'test' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(req.request.headers.get('X-CSRF-TOKEN')).toBe('initial-csrf-token');
    req.flush({});
  });

  it('refreshes csrf token and retries once after csrf 400', async () => {
    csrfTokenService.setToken('stale-csrf-token');

    const responsePromise = new Promise<void>((resolve) => {
      http
        .post(`${environment.apiUrl}saints`, { name: 'test' })
        .subscribe(() => resolve());
    });

    const firstReq = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(firstReq.request.headers.get('X-CSRF-TOKEN')).toBe(
      'stale-csrf-token',
    );
    firstReq.flush(
      { message: 'Invalid or missing CSRF token.' },
      { status: 400, statusText: 'Bad Request' },
    );

    // Retry is triggered via an async token refresh promise.
    await Promise.resolve();

    const retryReq = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(retryReq.request.headers.get('X-CSRF-TOKEN')).toBe(
      'refreshed-csrf-token',
    );
    retryReq.flush({});

    await responsePromise;
  });

  it('navigates to login on 401 errors', () => {
    http.get(`${environment.apiUrl}saints`).subscribe({ error: () => {} });

    const req = httpMock.expectOne(`${environment.apiUrl}saints`);
    req.flush('fail', { status: 401, statusText: 'Unauthorized' });

    expect(router.navigate).toHaveBeenCalledWith(['/account/login'], {
      queryParams: { returnUrl: '/admin' },
    });
  });
});
