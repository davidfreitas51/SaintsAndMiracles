import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { UserSessionService } from '../services/user-session.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private session: UserSessionService, private router: Router) {}

  canActivate(): Observable<boolean> {
    return this.session.initUser().pipe(
      map((user) => {
        if (user) return true;
        this.router.navigate(['/account/login']);
        return false;
      })
    );
  }
}
