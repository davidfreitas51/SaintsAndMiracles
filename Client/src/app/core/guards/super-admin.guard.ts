import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { UserSessionService } from '../services/user-session.service';

@Injectable({ providedIn: 'root' })
export class SuperAdminGuard implements CanActivate {
  constructor(
    private session: UserSessionService,
    private router: Router,
  ) {}

  canActivate(): Observable<boolean> {
    return this.session.initUser().pipe(
      map(() => {
        const role = this.session.getUserRole();
        const isSuperAdmin = role === 'SuperAdmin';
        if (!isSuperAdmin) {
          this.router.navigate(['admin/dashboard']);
        }
        return isSuperAdmin;
      }),
    );
  }
}
