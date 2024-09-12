import { Component, OnInit } from '@angular/core';
import { UserProfileService } from '../../services/user-profile.service';
import { TradingService } from '../../services/trading.service';
import { Router } from '@angular/router';
import { PortfolioItem } from 'src/app/models/portfolio-item.model';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent implements OnInit {
  userProfile: any;
  portfolioDetails: any[] = [];
  transactions: any[] = [];
  totalPortfolioValue: number = 0;

  constructor(
    private userProfileService: UserProfileService,
    private tradingService: TradingService
  ) {}

  ngOnInit(): void {
    const username = sessionStorage.getItem('username');
    if (username) {
      this.loadUserProfile(username);
      this.loadUserPortfolio(username);
      this.loadUserPortfolioValue(username);
      this.loadTransactionHistory(username);
    }
  }

  loadUserProfile(username: string): void {
    this.userProfileService.getUserProfile(username).subscribe(
      (data) => {
        this.userProfile = data;
        if (this.userProfile.country == "SRB") this.userProfile.country = "Serbia (SRB)";
        else if (this.userProfile.country == "MNE") this.userProfile.country = "Montenegro (MNE)";
        else this.userProfile.country = "Bosnia and Herzegovina (BIH)";
      },
      (error) => {
        console.error('Error fetching user profile:', error);
      }
    );
  }

  loadUserPortfolio(username: string): void {
    this.userProfileService.getUserPortfolio(username).subscribe(
      (data) => {
        this.portfolioDetails = Object.keys(data).map(symbol => ({
          symbol,
          quantity: data[symbol].quantity,
          expanded: false
        }));
      },
      (error) => {
        console.error('Error fetching user portfolio:', error);
      }
    );
  }

  loadUserPortfolioValue(username: string): void {
    this.userProfileService.getUserPortfolioValue(username).subscribe(
      (value) => {
        this.totalPortfolioValue = value;
      },
      (error) => {
        console.error('Error fetching user portfolio value:', error);
      }
    );
  }

  loadTransactionHistory(username: string): void {
    this.tradingService.getTransactionHistory(username).subscribe(
      (data) => {
        this.transactions = data;
      },
      (error) => {
        console.error('Error fetching transaction history:', error);
      }
    );
  }

  getTransactionsForSymbol(symbol: string): any[] {
    return this.transactions.filter(transaction => transaction.stockSymbol === symbol);
  }

  toggleExpand(item: any): void {
    item.expanded = !item.expanded;
  }
}