import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardSummaryDto } from '../../features/admin/interfaces/dashboard-summary-dto';
import { PagedRecentActivity } from '../../features/admin/interfaces/paged-activities';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  getSummary(): Observable<DashboardSummaryDto> {
    return this.http.get<DashboardSummaryDto>(
      `${this.baseUrl}dashboard/summary`
    );
  }

  getRecentActivities(pageNumber: number, pageSize: number) {
    return this.http.get<PagedRecentActivity>(
      `${this.baseUrl}dashboard/recent`,
      {
        params: {
          pageNumber: pageNumber.toString(),
          pageSize: pageSize.toString(),
        },
      }
    );
  }
}
