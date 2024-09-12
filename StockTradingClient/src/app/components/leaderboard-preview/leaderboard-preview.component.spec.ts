import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LeaderboardPreviewComponent } from './leaderboard-preview.component';

describe('LeaderboardPreviewComponent', () => {
  let component: LeaderboardPreviewComponent;
  let fixture: ComponentFixture<LeaderboardPreviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [LeaderboardPreviewComponent]
    });
    fixture = TestBed.createComponent(LeaderboardPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
