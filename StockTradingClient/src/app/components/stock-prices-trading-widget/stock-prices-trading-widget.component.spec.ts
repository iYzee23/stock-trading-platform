import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockPricesTradingWidgetComponent } from './stock-prices-trading-widget.component';

describe('StockPricesTradingWidgetComponent', () => {
  let component: StockPricesTradingWidgetComponent;
  let fixture: ComponentFixture<StockPricesTradingWidgetComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [StockPricesTradingWidgetComponent]
    });
    fixture = TestBed.createComponent(StockPricesTradingWidgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
