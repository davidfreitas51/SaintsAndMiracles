import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
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
    this.loadStoredUser();
  }

  private saveUser(user: CurrentUser | null): void {
    if (user) {
      localStorage.setItem('currentUser', JSON.stringify(user));
    } else {
      localStorage.removeItem('currentUser');
    }
  }

  private loadStoredUser(): void {
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`);
  }

  public login(login: LoginDto): Observable<CurrentUser> {
    const params = new HttpParams().set('useCookies', true);

    return this.http
      .post<CurrentUser>(`${this.baseUrl}accounts/login`, login, { params })
      .pipe(
        tap((user) => {
          this.currentUserSubject.next(user);
          this.saveUser(user);
        })
      );
  }

  public logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}accounts/logout`, null).pipe(
      tap(() => {
        this.currentUserSubject.next(null);
        this.saveUser(null);
      })
    );
  }

  public generateInviteToken(): Observable<string> {
    return this.http.post(`${this.baseUrl}accounts/invite`, null, {
      responseType: 'text',
    });
  }

  public resendConfirmation(email: string): Observable<any> {
    return this.http.post(`${this.baseUrl}accounts/resend-confirmation`, { email });
  }

  public register(values: any): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}accounts/register`, values);
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }
}
