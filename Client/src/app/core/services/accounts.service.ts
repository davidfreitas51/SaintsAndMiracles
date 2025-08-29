import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginDto } from '../../features/account/interfaces/login-dto';

export interface CurrentUser {
  firstName: string;
  lastName: string;
  email: string;
}

@Injectable({
  providedIn: 'root',
})
export class AccountsService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.checkCurrentUser().subscribe();
  }

  private checkCurrentUser(): Observable<CurrentUser | null> {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accounts/current-user`, {
        withCredentials: true,
      })
      .pipe(
        tap({
          next: (user) => this.currentUserSubject.next(user),
          error: () => this.currentUserSubject.next(null),
        })
      );
  }

  public login(login: LoginDto): Observable<CurrentUser> {
    const params = new HttpParams().set('useCookies', true);

    return this.http
      .post<CurrentUser>(`${this.baseUrl}accounts/login`, login, {
        params,
        withCredentials: true,
      })
      .pipe(
        tap((user) => {
          this.currentUserSubject.next(user);
        })
      );
  }

  public logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}accounts/logout`, null, {
        withCredentials: true,
      })
      .pipe(
        tap(() => {
          this.currentUserSubject.next(null);
        })
      );
  }

  public register(values: any): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}accounts/register`, values, {
      withCredentials: true,
    });
  }

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`, {
      withCredentials: true,
    });
  }

  public generateInviteToken(): Observable<string> {
    return this.http.post(`${this.baseUrl}accounts/invite`, null, {
      responseType: 'text',
      withCredentials: true,
    });
  }

  public resendConfirmation(email: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}accounts/resend-confirmation`,
      { email },
      { withCredentials: true }
    );
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  public isLoggedIn(): boolean {
    return this.currentUserSubject.value !== null;
  }
}
