import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockPricesWidgetComponent } from './stock-prices-widget.component';

describe('StockPricesWidgetComponent', () => {
  let component: StockPricesWidgetComponent;
  let fixture: ComponentFixture<StockPricesWidgetComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [StockPricesWidgetComponent]
    });
    fixture = TestBed.createComponent(StockPricesWidgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
