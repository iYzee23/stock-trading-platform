import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioSummaryWidgetComponent } from './portfolio-summary-widget.component';

describe('PortfolioSummaryWidgetComponent', () => {
  let component: PortfolioSummaryWidgetComponent;
  let fixture: ComponentFixture<PortfolioSummaryWidgetComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PortfolioSummaryWidgetComponent]
    });
    fixture = TestBed.createComponent(PortfolioSummaryWidgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
