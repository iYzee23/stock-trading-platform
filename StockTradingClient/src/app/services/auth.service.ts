import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // private apiUrl = 'http://localhost:8520/api/auth';
  private apiUrl = 'http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api/auth';

  constructor(private http: HttpClient) {}

  register(username: string, name: string, password: string, email: string, country: string): Observable<any> {
    const registerDto = {
      Username: username,
      Name: name,
      Password: password,
      Email: email,
      Country: country
    };

    return this.http.post<string>(`${this.apiUrl}/register`, registerDto, {
      responseType: 'text' as 'json'
    });
  }

  login(username: string, password: string): Observable<any> {
    const loginDto = {
      Username: username,
      Password: password
    };

    return this.http.post<any>(`${this.apiUrl}/login`, loginDto);
  }

  validateToken(token: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/validate-token`, { headers: { Authorization: `Bearer ${token}` } });
  }

  logout(): void {
    sessionStorage.removeItem('authToken');
    sessionStorage.removeItem('username');
  }
}
