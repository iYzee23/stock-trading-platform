import { Component, OnInit } from '@angular/core';
import { LeaderboardService } from '../../services/leaderboard.service';
import { Router } from '@angular/router';
import { SignalrService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-leaderboard',
  templateUrl: './leaderboard.component.html',
  styleUrls: ['./leaderboard.component.css']
})
export class LeaderboardComponent implements OnInit {
  leaderboard: Array<{ rank: number, username: string, portfolioValue: number }> = [];

  constructor(private leaderboardService: LeaderboardService, private router: Router, private signalrService: SignalrService) {}

  ngOnInit(): void {
    this.loadLeaderboard();
  }

  loadLeaderboard(): void {
    this.signalrService.leaderboard$.subscribe(
      (data) => {
        this.leaderboard = data
          .filter(entry => entry.Value && entry.Value.length > 0)
          .map((entry, index) => {
            return {
              rank: index + 1,
              username: entry.Value[0],
              portfolioValue: entry.Key
            };
          });
      },
      (error) => {
        if (error.status === 401) {
          console.error('Unauthorized access - invalid or expired token');
          this.router.navigate(['/login']);
        } else {
          console.error('Error fetching leaderboard data', error);
        }
      }
    );
  }
}
