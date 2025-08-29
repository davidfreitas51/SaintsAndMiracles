import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { CurrentUser } from '../../interfaces/current-user';

@Injectable({
  providedIn: 'root',
})
export class UserSessionService {
  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  public setUser(user: CurrentUser | null): void {
    this.currentUserSubject.next(user);
  }

  public getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  public isLoggedIn(): boolean {
    return this.currentUserSubject.value !== null;
  }
}
