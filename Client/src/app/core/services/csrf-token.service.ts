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
    try {
      const response = await firstValueFrom(
        this.http.get<{ token?: string }>(
          `${environment.apiUrl}security/csrf-token`,
          { withCredentials: true },
        ),
      );

      this.token = response?.token ?? null;
    } catch {
      this.token = null;
    }
  }
}
