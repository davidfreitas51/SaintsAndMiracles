import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class RegistrationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  public register(values: any): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}registration/register`, values, {
      withCredentials: true,
    });
  }

  public resendConfirmation(email: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}registration/resend-confirmation`,
      { email },
      { withCredentials: true }
    );
  }

  public generateInviteToken(): Observable<string> {
    return this.http.post(`${this.baseUrl}registration/invite`, null, {
      responseType: 'text',
      withCredentials: true,
    });
  }
}
