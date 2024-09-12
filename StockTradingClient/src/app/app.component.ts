import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { SignalrService } from './services/signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title: string = 'StockTradingClient';
  isLoggedIn: boolean = false;

  constructor(private router: Router, private cdRef: ChangeDetectorRef, private signalrService: SignalrService) {}

  ngOnInit(): void {
    this.checkLoginStatus();

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.checkLoginStatus();
      }
    });

    this.signalrService.startConnection();
    this.signalrService.addStockListener();
    this.signalrService.addLeaderboardListener();
  }

  checkLoginStatus(): void {
    this.isLoggedIn = !!sessionStorage.getItem('authToken');
    this.cdRef.detectChanges();
  }
}
