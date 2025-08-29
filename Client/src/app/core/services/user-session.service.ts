import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { CurrentUser } from '../../interfaces/current-user';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserSessionService {
  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  constructor() {
    this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .subscribe({
        next: (user) => this.currentUserSubject.next(user),
        error: () => this.currentUserSubject.next(null),
      });
  }

  public setUser(user: CurrentUser | null): void {
    this.currentUserSubject.next(user);
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  public isLoggedIn(): boolean {
    return this.currentUserSubject.value !== null;
  }

  public refreshCurrentUser() {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {
        withCredentials: true,
      })
      .pipe(
        map((user) => {
          this.currentUserSubject.next(user);
          return user;
        }),
        catchError(() => {
          this.currentUserSubject.next(null);
          return of(null);
        })
      );
  }
}
