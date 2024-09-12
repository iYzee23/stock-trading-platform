import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { LeaderboardComponent } from './pages/leaderboard/leaderboard.component';
import { TradingComponent } from './pages/trading/trading.component';
import { UserProfileComponent } from './pages/user-profile/user-profile.component';
import { HeaderComponent } from './shared/header/header.component';
import { NavComponent } from './shared/nav/nav.component';
import { StockPricesWidgetComponent } from './components/stock-prices-widget/stock-prices-widget.component';
import { PortfolioSummaryWidgetComponent } from './components/portfolio-summary-widget/portfolio-summary-widget.component';
import { LeaderboardPreviewComponent } from './components/leaderboard-preview/leaderboard-preview.component';
import { FormsModule } from '@angular/forms';
import { StockPricesComponent } from './pages/stock-prices/stock-prices.component';
import { StockPricesTradingWidgetComponent } from './components/stock-prices-trading-widget/stock-prices-trading-widget.component';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    LoginComponent,
    RegisterComponent,
    LeaderboardComponent,
    TradingComponent,
    UserProfileComponent,
    HeaderComponent,
    NavComponent,
    StockPricesWidgetComponent,
    PortfolioSummaryWidgetComponent,
    LeaderboardPreviewComponent,
    StockPricesComponent,
    StockPricesTradingWidgetComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
