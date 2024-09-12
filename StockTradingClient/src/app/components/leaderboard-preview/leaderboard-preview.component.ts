import { Component, OnInit } from '@angular/core';
import { LeaderboardService } from '../../services/leaderboard.service';
import { SignalrService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-leaderboard-preview',
  templateUrl: './leaderboard-preview.component.html',
  styleUrls: ['./leaderboard-preview.component.css']
})
export class LeaderboardPreviewComponent implements OnInit {
  leaderboard: Array<{ rank: number, username: string, portfolioValue: number }> = [];

  constructor(private leaderboardService: LeaderboardService, private signalrService: SignalrService) {}

  ngOnInit(): void {
    this.loadLeaderboardPreview();
  }

  loadLeaderboardPreview(): void {
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
          })
          .slice(0, 5);
      },
      (error) => {
        console.error('Error fetching leaderboard data', error);
      }
    );
  }
}
