import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';
import { CurrentUser } from '../../interfaces/current-user';
import { UserSessionService } from './user-session.service';
import { ResetPasswordDto } from '../../features/account/interfaces/reset-password-dto';

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
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {})
      .pipe(
        tap({
          next: (user) => this.session.setUser(user),
          error: () => this.session.setUser(null),
        })
      );
  }

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`, {});
  }

  public requestPasswordReset(email: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/forgot-password`,
      { email }
    );
  }

  resetPassword(dto: ResetPasswordDto): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/reset-password`,
      dto,
      { withCredentials: true }
    );
  }
}
