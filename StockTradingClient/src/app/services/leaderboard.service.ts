import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LeaderboardService {
  // private apiUrl = 'http://localhost:8520/api/leaderboard';
  private apiUrl = 'http://localhost:19081/StockTradingPlatform/PlatformAPIV2/api/leaderboard';

  constructor(private http: HttpClient) {}

  getLeaderboard(): Observable<any[]> {
    const token = sessionStorage.getItem('authToken');

    return this.http.get<any[]>(`${this.apiUrl}`, {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    });
  }

}
