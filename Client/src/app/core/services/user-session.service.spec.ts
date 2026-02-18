import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { UserSessionService } from './user-session.service';
import { environment } from '../../../environments/environment';
import { createCurrentUser } from '../../../testing/test-fixtures';

describe('UserSessionService', () => {
  let service: UserSessionService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(UserSessionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('initializes user and role on success', () => {
    const user = createCurrentUser();
    let result: typeof user | null | undefined;

    service.initUser().subscribe((value) => (result = value));

    const userReq = httpMock.expectOne(
      `${environment.apiUrl}accountManagement/current-user`,
    );
    expect(userReq.request.withCredentials).toBeTrue();
    userReq.flush(user);

    const roleReq = httpMock.expectOne(
      `${environment.apiUrl}accountManagement/user-role`,
    );
    expect(roleReq.request.withCredentials).toBeTrue();
    roleReq.flush({ role: 'Admin' });

    expect(result).toEqual(user);
    expect(service.getCurrentUser()).toEqual(user);
    expect(service.getUserRole()).toBe('Admin');
  });

  it('clears session on init failure and retries', () => {
    service.initUser().subscribe();

    const userReq = httpMock.expectOne(
      `${environment.apiUrl}accountManagement/current-user`,
    );
    userReq.flush('fail', { status: 401, statusText: 'Unauthorized' });

    expect(service.getCurrentUser()).toBeNull();
    expect(service.getUserRole()).toBeNull();

    service.initUser().subscribe();

    const retryReq = httpMock.expectOne(
      `${environment.apiUrl}accountManagement/current-user`,
    );
    retryReq.flush(createCurrentUser());

    const roleReq = httpMock.expectOne(
      `${environment.apiUrl}accountManagement/user-role`,
    );
    roleReq.flush({ role: 'User' });
  });
});
