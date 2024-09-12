import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { StockPriceModel } from '../models/stock-price.model';
import { LeaderboardEntry } from '../models/leaderboard-entry.model';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  private stockPricesSource = new BehaviorSubject<{ [symbol: string]: StockPriceModel }>({});
  stockPrices$ = this.stockPricesSource.asObservable();
  private leaderboardSource = new BehaviorSubject<LeaderboardEntry[]>([]);
  leaderboard$ = this.leaderboardSource.asObservable();

  constructor() {}

  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      // .withUrl('http://localhost:8676/stockHub')
      .withUrl('http://localhost:19081/StockTradingPlatform/TRealTimeBroadcaster/stockHub')
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public addStockListener() {
    this.hubConnection.on('StockUpdates', (aggregatedPricesJson: string) => {
      const stocks: { [symbol: string]: StockPriceModel } = JSON.parse(aggregatedPricesJson);
      this.stockPricesSource.next(stocks);
    });

    this.hubConnection.on('InitialStockUpdates', (aggregatedPricesJson: string) => {
      const stocks: { [symbol: string]: StockPriceModel } = JSON.parse(aggregatedPricesJson);
      this.stockPricesSource.next(stocks);
    });
  }

  public addLeaderboardListener() {
    this.hubConnection.on('LeaderboardUpdates', (leaderboardJson: string) => {
      const leaderboard: LeaderboardEntry[] = JSON.parse(leaderboardJson);
      this.leaderboardSource.next(leaderboard);
    });

    this.hubConnection.on('InitialLeaderboardUpdates', (leaderboardJson: string) => {
      const leaderboard: LeaderboardEntry[] = JSON.parse(leaderboardJson);
      this.leaderboardSource.next(leaderboard);
    });
  }
}
