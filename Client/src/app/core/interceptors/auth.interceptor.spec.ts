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

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: ReturnType<typeof createRouterSpy>;

  beforeEach(() => {
    router = createRouterSpy('/admin');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('adds credentials for API calls', () => {
    http.get(`${environment.apiUrl}saints`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(req.request.withCredentials).toBeTrue();
    req.flush([]);
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
