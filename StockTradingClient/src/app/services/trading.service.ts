import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TradingService {
  // private apiUrl = 'http://localhost:8520/api/trade';
  private apiUrl = 'http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api/trade';

  constructor(private http: HttpClient) {}

  buyStock(username: string, stockSymbol: string, quantity: number): Observable<any> {
    const tradeRequestDto = {
      Username: username,
      StockSymbol: stockSymbol,
      Quantity: quantity
    };

    const token = sessionStorage.getItem('authToken');

    return this.http.post<any>(`${this.apiUrl}/buy`, tradeRequestDto, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      }),
      responseType: 'text' as 'json'
    });
  }

  sellStock(username: string, stockSymbol: string, quantity: number): Observable<any> {
    const tradeRequestDto = {
      Username: username,
      StockSymbol: stockSymbol,
      Quantity: quantity
    };

    const token = sessionStorage.getItem('authToken');

    return this.http.post<any>(`${this.apiUrl}/sell`, tradeRequestDto, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      }),
      responseType: 'text' as 'json'
    });
  }

  getTransactionHistory(username: string): Observable<any[]> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<any[]>(`${this.apiUrl}/history/${username}`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }

}
