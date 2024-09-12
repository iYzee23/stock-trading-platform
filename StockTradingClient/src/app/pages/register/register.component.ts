import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  email: string = '';
  name: string = '';
  username: string = '';
  password: string = '';
  confirmPassword: string = '';
  country: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (this.password !== this.confirmPassword) {
      alert('Passwords do not match');
      return;
    }

    this.authService.register(this.username, this.name, this.password, this.email, this.country).subscribe(
      (response) => {
        alert(response);
        if (response === "Registration successful") {
          this.router.navigate(['/login']);
        }
        else {
          this.email = '';
          this.name = '';
          this.username = '';
          this.password = '';
          this.confirmPassword = '';
          this.country = '';
        }
      },
      (error) => {
        console.error('Registration failed', error);
      }
    );
  }
}
