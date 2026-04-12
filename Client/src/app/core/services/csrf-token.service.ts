import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CsrfTokenService {
  private readonly http = inject(HttpClient);
  private token: string | null = null;

  public getToken(): string | null {
    return this.token;
  }

  public async initializeToken(): Promise<void> {
    this.token = await this.fetchToken();
  }

  public async refreshToken(): Promise<string | null> {
    this.token = await this.fetchToken();
    return this.token;
  }

  private async fetchToken(): Promise<string | null> {
    try {
      const response = await firstValueFrom(
        this.http.get<{ token?: string }>(
          `${environment.apiUrl}security/csrf-token`,
          { withCredentials: true },
        ),
      );

      return response?.token ?? null;
    } catch {
      return null;
    }
  }
}
