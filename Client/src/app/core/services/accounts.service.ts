import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AccountsService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`);
  }

  public logoutUser() {
    this.http.post<void>(`${this.baseUrl}accounts/logout`, null);
  }
}
