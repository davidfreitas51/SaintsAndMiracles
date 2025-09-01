import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, switchMap, map } from 'rxjs';
import { Saint } from '../../features/saints/interfaces/saint';
import { NewSaintDto } from '../../features/saints/interfaces/new-saint-dto';
import { SaintFilters } from '../../features/saints/interfaces/saint-filter';
import { SaintWithMarkdown } from '../../features/saints/interfaces/saint-with-markdown';

@Injectable({ providedIn: 'root' })
export class SaintsService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getSaints(
    filters: SaintFilters
  ): Observable<{ items: Saint[]; totalCount: number }> {
    let params = new HttpParams();

    for (const [key, value] of Object.entries(filters)) {
      if (
        value === null ||
        value === undefined ||
        value === '' ||
        (Array.isArray(value) && value.length === 0)
      )
        continue;

      if (Array.isArray(value)) {
        value.forEach((val) => (params = params.append(key, val.toString())));
      } else {
        params = params.set(key, value.toString());
      }
    }

    return this.http.get<{ items: Saint[]; totalCount: number }>(
      `${this.baseUrl}saints`,
      { params }
    );
  }

  getSaint(slug: string): Observable<Saint> {
    return this.http.get<Saint>(`${this.baseUrl}saints/${slug}`);
  }

  getCountries(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}saints/countries`);
  }

  createSaint(formValue: any): Observable<void> {
    const saintDto: NewSaintDto = {
      ...formValue,
      century: Number(formValue.century),
      title: formValue.title || null,
      patronOf: formValue.patronOf || null,
      religiousOrderId: formValue.religiousOrder || null,
      tagIds: formValue.currentTags || [],
      feastDay: this.formatFeastDayToIso(formValue.feastDay),
    };

    return this.http.post<void>(`${this.baseUrl}saints`, saintDto);
  }

  updateSaint(
    id: string,
    formValue: NewSaintDto & { feastDay?: string }
  ): Observable<void> {
    const payload = {
      ...formValue,
      feastDay: this.formatFeastDayToIso(formValue.feastDay),
    };

    return this.http.put<void>(`${this.baseUrl}saints/${id}`, payload);
  }

  deleteSaint(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}saints/${id}`);
  }

  getSaintWithMarkdown(slug: string): Observable<SaintWithMarkdown> {
    return this.getSaint(slug).pipe(
      switchMap((saint) =>
        this.http
          .get(`${environment.assetsUrl}${saint.markdownPath}`, {
            responseType: 'text',
          })
          .pipe(map((markdown) => ({ saint, markdown })))
      )
    );
  }

  getSaintsOfTheDay(): Observable<Saint[]> {
    return this.http.get<Saint[]>(`${this.baseUrl}saints/of-the-day`);
  }

  private parseFeastDay(
    feastDay?: string | null
  ): { day: number; month: number } | null {
    if (!feastDay) return null;

    const digits = feastDay.replace(/\D/g, '');

    let day: number, month: number;

    if (digits.length === 4) {
      day = parseInt(digits.slice(0, 2), 10);
      month = parseInt(digits.slice(2, 4), 10);
    } else {
      const m = /^(\d{1,2})[\/\-.](\d{1,2})$/.exec(feastDay);
      if (!m) return null;
      day = parseInt(m[1], 10);
      month = parseInt(m[2], 10);
    }

    if (Number.isNaN(day) || Number.isNaN(month)) return null;
    if (day < 1 || day > 31) return null;
    if (month < 1 || month > 12) return null;

    return { day, month };
  }

  private formatFeastDayToIso(feastDay?: string | null): string | null {
    const parts = this.parseFeastDay(feastDay);
    if (!parts) return null;
    const dd = String(parts.day).padStart(2, '0');
    const mm = String(parts.month).padStart(2, '0');
    return `0001-${mm}-${dd}`;
  }

  public formatFeastDayFromIso(
    feastDayIso: string | null,
    opts?: { raw?: boolean }
  ): string | null {
    if (!feastDayIso) return null;
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(feastDayIso);
    if (!m) return null;
    const dd = m[3];
    const mm = m[2];
    return opts?.raw ? `${dd}${mm}` : `${dd}/${mm}`;
  }
}
