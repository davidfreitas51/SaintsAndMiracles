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
import { loadingInterceptor } from './loading.interceptor';
import { LoadingService } from '../services/loading.service';
import { environment } from '../../../environments/environment';

describe('loadingInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let loading: jasmine.SpyObj<LoadingService>;

  beforeEach(() => {
    loading = jasmine.createSpyObj<LoadingService>('LoadingService', [
      'show',
      'hide',
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([loadingInterceptor])),
        provideHttpClientTesting(),
        { provide: LoadingService, useValue: loading },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('shows and hides loading around requests', () => {
    http.get(`${environment.apiUrl}saints`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}saints`);
    expect(loading.show).toHaveBeenCalledTimes(1);

    req.flush([]);

    expect(loading.hide).toHaveBeenCalledTimes(1);
  });
});
