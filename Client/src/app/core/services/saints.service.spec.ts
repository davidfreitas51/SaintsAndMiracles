import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { SaintsService } from './saints.service';
import { SaintFilters } from '../../features/saints/interfaces/saint-filter';
import { environment } from '../../../environments/environment';

describe('SaintsService', () => {
  let service: SaintsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(SaintsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('builds query params from filters', () => {
    const filters = new SaintFilters();
    filters.country = 'Italy';
    filters.century = 3;
    filters.tagIds = [1, 2];
    filters.pageNumber = 2;
    filters.pageSize = 10;
    filters.religiousOrderId = 5;

    service.getSaints(filters).subscribe();

    const req = httpMock.expectOne(
      (request) => request.url === `${environment.apiUrl}saints`,
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('country')).toBe('Italy');
    expect(req.request.params.get('century')).toBe('3');
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('religiousOrderId')).toBe('5');
    expect(req.request.params.getAll('tagIds')).toEqual(['1', '2']);
    expect(req.request.params.has('search')).toBeFalse();

    req.flush({ items: [], totalCount: 0 });
  });

  it('formats feast days to and from ISO', () => {
    expect(service.formatFeastDayToIso('1/2')).toBe('0001-02-01');
    expect(service.formatFeastDayToIso('31-12')).toBe('0001-12-31');
    expect(service.formatFeastDayToIso('0001-08-28')).toBe('0001-08-28');
    expect(service.formatFeastDayToIso('invalid')).toBeUndefined();

    expect(service.formatFeastDayFromIso('0001-12-25')).toBe('25/12');
    expect(service.formatFeastDayFromIso('1999-01-02')).toBe('02/01');
    expect(service.formatFeastDayFromIso('')).toBeUndefined();
  });
});
