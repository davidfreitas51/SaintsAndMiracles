import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser } from '../../interfaces/current-user';

@Injectable({ providedIn: 'root' })
export class UserSessionService {
  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  private initialized = false;

  public initUser(): Observable<CurrentUser | null> {
    if (this.initialized) {
      return of(this.currentUserSubject.value);
    }

    this.initialized = true;

    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .pipe(
        tap((user) => {
          this.currentUserSubject.next(user);
        }),
        catchError(() => {
          this.currentUserSubject.next(null);
          return of(null);
        })
      );
  }

  public setUser(user: CurrentUser | null): void {
    this.currentUserSubject.next(user);
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  public refreshCurrentUser(): Observable<CurrentUser | null> {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .pipe(
        tap((user) => {
          this.currentUserSubject.next(user);
          console.log('RefreshCurrentUser:', user);
        }),
        catchError(() => {
          this.currentUserSubject.next(null);
          console.log('RefreshCurrentUser: não autorizado');
          return of(null);
        })
      );
  }

  public isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }
}
