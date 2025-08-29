import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';
import { LoginDto } from '../../features/account/interfaces/login-dto';
import { CurrentUser } from '../../interfaces/current-user';
import { UserSessionService } from './user-session.service';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private session = inject(UserSessionService);

  public login(login: LoginDto): Observable<CurrentUser> {
    const params = new HttpParams().set('useCookies', true);

    return this.http
      .post<CurrentUser>(`${this.baseUrl}authentication/login`, login, {
        params,
        withCredentials: true,
      })
      .pipe(
        tap((user) => {
          this.session.setUser(user);
        })
      );
  }

  public logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}authentication/logout`, null, {
        withCredentials: true,
      })
      .pipe(
        tap(() => {
          this.session.setUser(null);
        })
      );
  }
}
