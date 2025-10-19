import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser } from '../../interfaces/current-user';

@Injectable({ providedIn: 'root' })
export class UserSessionService {
  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private userRoleSubject = new BehaviorSubject<string | null>(null);
  public userRole$ = this.userRoleSubject.asObservable();

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
          this.setUser(user);
          this.fetchUserRole();
        }),
        catchError(() => {
          this.clearSession();
          return of(null);
        })
      );
  }

  public fetchUserRole(): void {
    this.http
      .get<{ role: string }>(`${this.baseUrl}accountManagement/user-role`, {
        withCredentials: true,
      })
      .pipe(
        tap({
          next: (res) => this.setUserRole(res.role),
          error: () => this.setUserRole(null),
        }),
        catchError(() => of(null))
      )
      .subscribe();
  }

  public setUser(user: CurrentUser | null): void {
    this.currentUserSubject.next(user);
  }

  public setUserRole(role: string | null): void {
    this.userRoleSubject.next(role);
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  public getUserRole(): string | null {
    return this.userRoleSubject.value;
  }

  public refreshCurrentUser(): Observable<CurrentUser | null> {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .pipe(
        tap((user) => {
          this.setUser(user);
          this.fetchUserRole();
        }),
        catchError(() => {
          this.clearSession();
          return of(null);
        })
      );
  }

  public isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  public clearSession(): void {
    this.setUser(null);
    this.setUserRole(null);
    this.initialized = false;
  }
}
