import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResetPasswordDto } from '../../features/account/interfaces/reset-password-dto';
import { CurrentUser } from '../../interfaces/current-user';
import { UserSessionService } from './user-session.service';
import { UpdateProfileDto } from '../../features/account/interfaces/update-profile-dto';
import { UserSummary } from '../../features/account/interfaces/user-summary';

@Injectable({
  providedIn: 'root',
})
export class AccountManagementService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private session = inject(UserSessionService);

  constructor() {}

  public checkCurrentUser() {
    return this.http
      .get<CurrentUser>(`${this.baseUrl}accountManagement/current-user`, {})
      .pipe(
        tap({
          next: (user) => this.session.setUser(user),
          error: () => this.session.setUser(null),
        }),
      );
  }

  public updateProfile(dto: UpdateProfileDto) {
    return this.http
      .put<CurrentUser>(
        `${this.baseUrl}accountManagement/update-profile`,
        dto,
        {},
      )
      .pipe(
        tap({
          next: (user) => this.session.setUser(user),
        }),
      );
  }

  public requestEmailChange(newEmail: string) {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/request-email-change`,
      { newEmail },
      { withCredentials: true },
    );
  }

  public changePassword(dto: { currentPassword: string; newPassword: string }) {
    return this.http.post(
      `${this.baseUrl}accountManagement/change-password`,
      dto,
    );
  }

  public requestPasswordReset(email: string) {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/forgot-password`,
      { email },
    );
  }

  public resetPassword(dto: ResetPasswordDto) {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/reset-password`,
      dto,
    );
  }

  public getAllUsers() {
    return this.http.get<UserSummary[]>(
      `${this.baseUrl}accountManagement/users`,
    );
  }

  public deleteUser(userEmail: string) {
    return this.http.delete<void>(
      `${this.baseUrl}accountManagement/users/${userEmail}`,
      {
        withCredentials: true,
      },
    );
  }
}
