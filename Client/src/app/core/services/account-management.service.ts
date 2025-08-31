import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { tap } from "rxjs";
import { environment } from "../../../environments/environment";
import { ResetPasswordDto } from "../../features/account/interfaces/reset-password-dto";
import { CurrentUser } from "../../interfaces/current-user";
import { UserSessionService } from "./user-session.service";

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
        })
      );
  }

  public deleteUser(id: number) {
    return this.http.delete<void>(`${this.baseUrl}accounts/${id}`, {});
  }

  public requestPasswordReset(email: string) {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/forgot-password`,
      { email }
    );
  }

  public resetPassword(dto: ResetPasswordDto) {
    return this.http.post<void>(
      `${this.baseUrl}accountManagement/reset-password`,
      dto
    );
  }
}
