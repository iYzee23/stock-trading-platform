import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StockService {
  // private apiUrl = 'http://localhost:8520/api/stocks';
  private apiUrl = 'http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api/stocks';

  constructor(private http: HttpClient) {}

  getLatestStockPrices(): Observable<any[]> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<any[]>(`${this.apiUrl}/latest`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }

  getSupportedSymbols(): Observable<string[]> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<string[]>(`${this.apiUrl}/symbols`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }
}
