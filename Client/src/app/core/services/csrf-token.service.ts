import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

interface CsrfTokenResponse {
  token?: string;
}

@Injectable({ providedIn: 'root' })
export class CsrfTokenService {
  private readonly http = inject(HttpClient);

  private token: string | null = null;
  private initialization?: Promise<void>;

  public getToken(): string | null {
    return this.token;
  }

  public initializeToken(): Promise<void> {
    if (this.token) {
      return Promise.resolve();
    }

    this.initialization ??= this.loadToken().catch((error) => {
      this.initialization = undefined;
      throw error;
    });

    return this.initialization;
  }

  public async refreshToken(): Promise<string> {
    this.token = null;
    this.initialization = undefined;

    await this.initializeToken();

    if (!this.token) {
      throw new Error('Não foi possível obter o token CSRF.');
    }

    return this.token;
  }

  private async loadToken(): Promise<void> {
    let lastError: unknown;

    for (let attempt = 1; attempt <= 5; attempt++) {
      try {
        const response = await firstValueFrom(
          this.http.get<CsrfTokenResponse>(
            `${environment.apiUrl}security/csrf-token`,
            { withCredentials: true },
          ),
        );

        if (!response?.token?.trim()) {
          throw new Error('A API retornou uma resposta sem token CSRF.');
        }

        this.token = response.token;
        return;
      } catch (error) {
        lastError = error;

        if (attempt < 5) {
          await new Promise((resolve) => setTimeout(resolve, 1000 * attempt));
        }
      }
    }

    throw lastError ?? new Error('Falha ao obter token CSRF.');
  }
}
