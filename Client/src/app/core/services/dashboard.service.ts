import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  getTotalSaints(): Observable<number> {
    return this.http.get<number>(this.baseUrl + 'dashboard/saints');
  }

  getTotalMiracles(): Observable<number> {
    return this.http.get<number>(this.baseUrl + 'dashboard/miracles');
  }

  getTotalPrayers(): Observable<number> {
    return this.http.get<number>(this.baseUrl + 'dashboard/prayers');
  }

  getTotalUsers(): Observable<number> {
    return this.http.get<number>(this.baseUrl + 'dashboard/users');
  }
}
