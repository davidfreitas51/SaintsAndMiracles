import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Prayer } from '../../features/prayers/interfaces/prayer';
import { PrayerFilters } from '../../features/prayers/interfaces/prayer-filter';
import { NewPrayerDto } from '../../features/prayers/interfaces/new-prayer-dto';
import { PrayerWithMarkdown } from '../../features/prayers/interfaces/prayer-with-markdown';

@Injectable({
  providedIn: 'root',
})
export class PrayersService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  getPrayers(
    filters: PrayerFilters
  ): Observable<{ items: Prayer[]; totalCount: number }> {
    let params = new HttpParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (
        value !== null &&
        value !== undefined &&
        value !== '' &&
        !(Array.isArray(value) && value.length === 0)
      ) {
        if (Array.isArray(value)) {
          value.forEach((val) => {
            params = params.append(key, val.toString());
          });
        } else {
          params = params.set(key, value.toString());
        }
      }
    });

    return this.http.get<{ items: Prayer[]; totalCount: number }>(
      this.baseUrl + 'prayers',
      { params }
    );
  }

  public getPrayerBySlug(slug: string): Observable<Prayer> {
    return this.http.get<Prayer>(`${this.baseUrl}prayers/${slug}`);
  }

  public createPrayer(formValue: any): Observable<void> {
    const prayerDto: NewPrayerDto = {
      title: formValue.title,
      description: formValue.description,
      image: formValue.image || null,
      markdownContent: formValue.markdownContent,
      tagIds: formValue.tagIds || [],
    };

    return this.http.post<void>(this.baseUrl + 'prayers', prayerDto);
  }

  public deletePrayer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}prayers/${id}`);
  }

  public updatePrayer(id: string, formValue: NewPrayerDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}prayers/${id}`, formValue);
  }

  public getPrayerWithMarkdown(slug: string): Observable<PrayerWithMarkdown> {
    return this.getPrayerBySlug(slug).pipe(
      switchMap((prayer) =>
        this.http
          .get(environment.assetsUrl + prayer.markdownPath, {
            responseType: 'text',
          })
          .pipe(map((markdown) => ({ prayer, markdown })))
      )
    );
  }

  public getMarkdownContent(path: string): Observable<string> {
    return this.http.get(environment.assetsUrl + path, {
      responseType: 'text',
    });
  }
}
