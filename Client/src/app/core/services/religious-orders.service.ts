import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EntityFilters } from '../../interfaces/entity-filters';
import { ReligiousOrder } from '../../interfaces/religious-order';

@Injectable({
  providedIn: 'root',
})
export class ReligiousOrdersService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'religious-orders';

  getOrders(
    filters: EntityFilters,
  ): Observable<{ items: ReligiousOrder[]; totalCount: number }> {
    let params = new HttpParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<{ items: ReligiousOrder[]; totalCount: number }>(
      this.baseUrl,
      {
        params,
      },
    );
  }

  getOrder(id: number): Observable<ReligiousOrder> {
    return this.http.get<ReligiousOrder>(`${this.baseUrl}/${id}`);
  }

  createOrder(name: string): Observable<void> {
    return this.http.post<void>(this.baseUrl, { name });
  }

  updateOrder(order: ReligiousOrder): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${order.id}`, {
      name: order.name,
    });
  }

  deleteOrder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
