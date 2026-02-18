import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { AuthenticationService } from './authentication.service';
import { UserSessionService } from './user-session.service';
import { environment } from '../../../environments/environment';
import { createCurrentUser } from '../../../testing/test-fixtures';

describe('AuthenticationService', () => {
  let service: AuthenticationService;
  let httpMock: HttpTestingController;
  let session: jasmine.SpyObj<UserSessionService>;

  beforeEach(() => {
    session = jasmine.createSpyObj<UserSessionService>('UserSessionService', [
      'setUser',
      'fetchUserRole',
      'clearSession',
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: UserSessionService, useValue: session },
      ],
    });

    service = TestBed.inject(AuthenticationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('logs in and updates the session', () => {
    const loginDto = {
      email: 'jane@example.com',
      password: 'secret',
      rememberMe: false,
    };
    const user = createCurrentUser({ email: 'jane@example.com' });

    service.login(loginDto).subscribe();

    const req = httpMock.expectOne(
      (request) => request.url === `${environment.apiUrl}authentication/login`,
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBeTrue();
    expect(req.request.params.get('useCookies')).toBe('true');
    req.flush(user);

    expect(session.setUser).toHaveBeenCalledWith(user);
    expect(session.fetchUserRole).toHaveBeenCalled();
  });

  it('logs out and clears the session', () => {
    service.logout().subscribe();

    const req = httpMock.expectOne(
      (request) => request.url === `${environment.apiUrl}authentication/logout`,
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBeTrue();
    req.flush(null);

    expect(session.clearSession).toHaveBeenCalled();
  });
});
