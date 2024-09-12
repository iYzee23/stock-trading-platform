import { Component, OnInit  } from '@angular/core';
import { StockService } from 'src/app/services/stock.service';
import { SignalrService } from 'src/app/services/signalr.service';
import { StockPriceModel } from 'src/app/models/stock-price.model';

@Component({
  selector: 'app-stock-prices-widget',
  templateUrl: './stock-prices-widget.component.html',
  styleUrls: ['./stock-prices-widget.component.css']
})
export class StockPricesWidgetComponent implements OnInit {
  stockPrices: { [symbol: string]: StockPriceModel } = {};
  visibleStocks: Array<{ key: string, value: StockPriceModel }> = [];
  startIndex: number = 0;
  visibleCount: number = 5;

  constructor(private signalrService: SignalrService) {}

  ngOnInit(): void {
    this.loadStockPrices();
  }

  loadStockPrices(): void {
    this.signalrService.stockPrices$.subscribe(prices => {
      this.stockPrices = prices;
      this.updateVisibleStocks();
    });
  }

  updateVisibleStocks(): void {
    const allStocks = Object.entries(this.stockPrices).map(([key, value]) => ({ key, value }));
    const length = allStocks.length;

    this.visibleStocks = allStocks.slice(this.startIndex, this.startIndex + this.visibleCount);

    if (this.visibleStocks.length < this.visibleCount) {
      this.visibleStocks = this.visibleStocks.concat(
        allStocks.slice(0, this.visibleCount - this.visibleStocks.length)
      );
    }
  }

  showPrevious(): void {
    this.startIndex = (this.startIndex - this.visibleCount + Object.keys(this.stockPrices).length) % Object.keys(this.stockPrices).length;
    this.updateVisibleStocks();
  }

  showNext(): void {
    this.startIndex = (this.startIndex + this.visibleCount) % Object.keys(this.stockPrices).length;
    this.updateVisibleStocks();
  }
}