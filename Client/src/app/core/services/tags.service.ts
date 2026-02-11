import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Tag } from '../../interfaces/tag';
import { EntityFilters, TagType } from '../../interfaces/entity-filters';
import { NewTagDto } from '../../interfaces/new-tag-dto';

@Injectable({
  providedIn: 'root',
})
export class TagsService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'tags';

  getTags(
    filters: EntityFilters,
  ): Observable<{ items: Tag[]; totalCount: number }> {
    let params = new HttpParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<{ items: Tag[]; totalCount: number }>(this.baseUrl, {
      params,
    });
  }

  getTag(id: number): Observable<Tag> {
    return this.http.get<Tag>(`${this.baseUrl}/${id}`);
  }

  createTag(name: string, tagType: TagType): Observable<void> {
    return this.http.post<void>(this.baseUrl, {
      name,
      tagType: tagType,
    });
  }

  updateTag(tag: Tag): Observable<void> {
    const dto: NewTagDto = {
      name: tag.name,
      tagType: 0,
    };
    return this.http.put<void>(`${this.baseUrl}/${tag.id}`, dto);
  }

  deleteTag(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
