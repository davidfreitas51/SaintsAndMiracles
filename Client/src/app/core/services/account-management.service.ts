import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';
import { CurrentUser } from '../../interfaces/current-user';
import { UserSessionService } from './user-session.service';

@Injectable({
  providedIn: 'root',
})
export class AccountManagementService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private session = inject(UserSessionService);

  constructor() {
    this.checkCurrentUser().subscribe();
  }

  private checkCurrentUser(): Observable<CurrentUser | null> {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .pipe(
        tap({
          next: (user) => this.session.setUser(user),
          error: () => this.session.setUser(null),
        })
      );
  }

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`, {
      withCredentials: true,
    });
  }
}
