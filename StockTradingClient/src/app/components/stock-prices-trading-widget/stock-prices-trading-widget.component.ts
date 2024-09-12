import { Component, OnInit } from '@angular/core';
import { StockPriceModel } from 'src/app/models/stock-price.model';
import { SignalrService } from 'src/app/services/signalr.service';
import { StockService } from 'src/app/services/stock.service';

@Component({
  selector: 'app-stock-prices-trading-widget',
  templateUrl: './stock-prices-trading-widget.component.html',
  styleUrls: ['./stock-prices-trading-widget.component.css']
})
export class StockPricesTradingWidgetComponent implements OnInit {
  stockPrices: { [symbol: string]: StockPriceModel } = {};

  constructor(private stockService: StockService, private signalrService: SignalrService) {}

  loadStockPrices(): void {
    this.signalrService.stockPrices$.subscribe(prices => {
      this.stockPrices = prices;
    });
  }

  ngOnInit(): void {
    this.loadStockPrices();
  }
}