import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, switchMap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Miracle } from '../../features/miracles/interfaces/miracle';
import { MiracleFilters } from '../../features/miracles/interfaces/miracle-filter';
import { NewMiracleDto } from '../../features/miracles/interfaces/new-miracle-dto';
import { MiracleWithMarkdown } from '../../features/miracles/interfaces/miracle-with-markdown';
import { Tag } from '../../interfaces/tag';

@Injectable({
  providedIn: 'root',
})
export class MiraclesService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  getMiracles(
    filters: MiracleFilters
  ): Observable<{ items: Miracle[]; totalCount: number }> {
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

    return this.http.get<{ items: Miracle[]; totalCount: number }>(
      this.baseUrl + 'miracles',
      { params }
    );
  }

  public getMiracleBySlug(slug: string): Observable<Miracle> {
    return this.http.get<Miracle>(`${this.baseUrl}miracles/${slug}`);
  }

  public getCountries(): Observable<string[]> {
    return this.http.get<string[]>(this.baseUrl + 'miracles/countries');
  }

  public createMiracle(formValue: any): Observable<void> {
    console.log(formValue.locationDetails);
    const miracleDto: NewMiracleDto = {
      title: formValue.title,
      country: formValue.country,
      century: +formValue.century,
      image: formValue.image,
      description: formValue.description,
      markdownContent: formValue.markdownContent,
      date: formValue.date || null,
      locationDetails: formValue.locationDetails || null,
      saintId: formValue.saintId || null,
      tagIds: formValue.tagIds || [],
    };

    console.log('location: ', miracleDto.locationDetails);
    return this.http.post<void>(this.baseUrl + 'miracles', miracleDto);
  }

  public deleteMiracle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}miracles/${id}`);
  }

  public updateMiracle(id: string, formValue: NewMiracleDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}miracles/${id}`, formValue);
  }

  public getMiracleWithMarkdown(slug: string): Observable<MiracleWithMarkdown> {
    return this.getMiracleBySlug(slug).pipe(
      switchMap((miracle) =>
        this.http
          .get(environment.assetsUrl + miracle.markdownPath, {
            responseType: 'text',
          })
          .pipe(map((markdown) => ({ miracle, markdown })))
      )
    );
  }

  public getMarkdownContent(path: string): Observable<string> {
    return this.http.get(environment.assetsUrl + path, {
      responseType: 'text',
    });
  }

  public getRecentMiracles(count: number = 5): Observable<Miracle[]> {
    return this.http.get<Miracle[]>(`${this.baseUrl}miracles/recent`, {
      params: new HttpParams().set('count', count),
    });
  }
}
