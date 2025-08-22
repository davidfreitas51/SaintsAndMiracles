import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { User } from '../../interfaces/user';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AccountService {
  public baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) {}

  login(values: any): Observable<User> {
    console.log('Attempting login with:', values);

    let params = new HttpParams().append('useCookies', true);

    return this.http
      .post<User>(this.baseUrl + 'login', values, {
        params,
        withCredentials: true,
      })
      .pipe(
        tap((user: User) => {
          console.log('Login response:', user);
          if (user) this.setCurrentUser(user);
        })
      );
  }

  setCurrentUser(user: User | null) {
    this.currentUserSource.next(user);
  }

  logout() {
    this.http
      .post(this.baseUrl + 'logout', {}, { withCredentials: true })
      .subscribe(() => {
        this.setCurrentUser(null);
      });
  }
}
