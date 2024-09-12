import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { LeaderboardComponent } from './pages/leaderboard/leaderboard.component';
import { TradingComponent } from './pages/trading/trading.component';
import { UserProfileComponent } from './pages/user-profile/user-profile.component';
import { authGuard } from './guards/auth.guard';
import { authReverseGuard } from './guards/auth-reverse.guard';
import { StockPricesComponent } from './pages/stock-prices/stock-prices.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent, canActivate: [authReverseGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [authReverseGuard] },
  { path: 'leaderboard', component: LeaderboardComponent, canActivate: [authGuard] },
  { path: 'trading', component: TradingComponent, canActivate: [authGuard] },
  { path: 'stock-prices', component: StockPricesComponent, canActivate: [authGuard] },
  { path: 'profile', component: UserProfileComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
