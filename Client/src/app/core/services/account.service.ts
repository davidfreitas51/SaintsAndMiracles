import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { User } from '../../interfaces/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  public baseUrl = environment.apiUrl;

  login(values: any) {
    let params = new HttpParams()
    params = params.append('useCookies', true)
    return this.http.post<User>(this.baseUrl + 'login', values, { params })
  }
}
