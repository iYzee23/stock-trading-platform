import { Component, OnInit } from '@angular/core';
import { TradingService } from '../../services/trading.service';
import { StockService } from 'src/app/services/stock.service';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-trading',
  templateUrl: './trading.component.html',
  styleUrls: ['./trading.component.css']
})
export class TradingComponent implements OnInit {
  transactions: any[] = [];
  symbol: string = '';
  shares: number = 1;
  supportedSymbols: string[] = [];

  constructor(private tradingService: TradingService, private stockService: StockService, private router: Router) {}

  ngOnInit(): void {
    this.loadTransactionHistory();
    this.loadSupportedSymbols();
  }

  onBuy(tradeForm: NgForm): void {
    if (tradeForm.valid) {
      const username = sessionStorage.getItem('username')!;
      this.tradingService.buyStock(username, this.symbol, this.shares).subscribe(
        response => {
          alert("The stock purchase order has been successfully created.");
          this.symbol = '';
          this.shares = 1;
          setTimeout(() => {
            this.loadTransactionHistory();
          }, 1000);
        },
        error => {
          if (error.status === 401) {
            console.error('Unauthorized access - invalid or expired token');
            this.router.navigate(['/login']);
          } else {
            console.error('Error buying stock', error);
          }
        }
      );
    }
  }

  onSell(tradeForm: NgForm): void {
    if (tradeForm.valid) {
      const username = sessionStorage.getItem('username')!;
      this.tradingService.sellStock(username, this.symbol, this.shares).subscribe(
        response => {
          alert("The stock selling order has been successfully created.");
          this.symbol = '';
          this.shares = 1;
          setTimeout(() => {
            this.loadTransactionHistory();
          }, 1000);
        },
        error => {
          if (error.status === 401) {
            console.error('Unauthorized access - invalid or expired token');
            this.router.navigate(['/login']);
          } else {
            console.error('Error selling stock', error);
          }
        }
      );
    }
  }

  loadTransactionHistory(): void {
    const username = sessionStorage.getItem('username')!;
    this.tradingService.getTransactionHistory(username).subscribe(
      data => {
        this.transactions = data;
      },
      error => {
        if (error.status === 401) {
          console.error('Unauthorized access - invalid or expired token');
          this.router.navigate(['/login']);
        } else {
          console.error('Error fetching transaction history', error);
        }
      }
    );
  }

  loadSupportedSymbols(): void {
    this.stockService.getSupportedSymbols().subscribe(
      symbols => this.supportedSymbols = symbols,
      error => console.error('Error fetching supported symbols:', error)
    );
  }
}
