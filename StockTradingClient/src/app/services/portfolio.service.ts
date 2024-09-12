import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PortfolioService {
  // private apiUrl = 'http://localhost:8520/api/users';
  private apiUrl = 'http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api/users';

  constructor(private http: HttpClient) {}

  getUserPortfolioSummary(username: string): Observable<any> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<any>(`${this.apiUrl}/${username}/portfolio`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }

  getUserPortfolioValue(username: string): Observable<number> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<number>(`${this.apiUrl}/${username}/portfolio/value`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }
}
