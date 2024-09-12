import { Component, OnInit } from '@angular/core';
import { StockPriceModel } from 'src/app/models/stock-price.model';
import { SignalrService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-stock-prices',
  templateUrl: './stock-prices.component.html',
  styleUrls: ['./stock-prices.component.css']
})
export class StockPricesComponent implements OnInit {
  stocks: { [symbol: string]: StockPriceModel } = {};

  constructor(private signalrService: SignalrService) {}

  ngOnInit(): void {
    this.loadStockPrices();
  }

  loadStockPrices(): void {
    this.signalrService.stockPrices$.subscribe(
      (data) => {
        this.stocks = data;
      },
      (error) => {
        console.error('Error fetching stock prices', error);
      }
    );
  }
}