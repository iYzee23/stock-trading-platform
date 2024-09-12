import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, ActivatedRoute  } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  username: string = '';
  password: string = '';
  returnUrl: string = '';

  constructor(private authService: AuthService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    this.authService.login(this.username, this.password).subscribe(
      (response) => {
        const token = response.token;
        sessionStorage.setItem('authToken', token);
        sessionStorage.setItem('username', this.username);
        this.router.navigateByUrl(this.returnUrl);
      },
      (error) => {
        console.error('Login failed', error);
      }
    );
  }
}
