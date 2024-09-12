import { Component, OnInit } from '@angular/core';
import { PortfolioService } from '../../services/portfolio.service';
import { PortfolioItem } from 'src/app/models/portfolio-item.model';

@Component({
  selector: 'app-portfolio-summary-widget',
  templateUrl: './portfolio-summary-widget.component.html',
  styleUrls: ['./portfolio-summary-widget.component.css']
})
export class PortfolioSummaryWidgetComponent implements OnInit {
  portfolio: { [key: string]: PortfolioItem } = {};
  portfolioValue: number = 0;

  constructor(private portfolioService: PortfolioService) {}

  loadPortfolioSummary(): void {
    const username = sessionStorage.getItem('username');
    if (!username) {
      console.error('Username not found in localStorage');
      return;
    }

    this.portfolioService.getUserPortfolioSummary(username).subscribe(
      data => {
        this.portfolio = data;
      },
      error => {
        console.error('Error fetching portfolio summary', error);
      }
    );

    this.portfolioService.getUserPortfolioValue(username).subscribe(
      value => {
        this.portfolioValue = value;
      },
      error => {
        console.error('Error fetching portfolio value', error);
      }
    );
  }

  ngOnInit(): void {
    this.loadPortfolioSummary();
  }
}
