import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterLink } from '@angular/router';
import { AccountService } from '../../../../core/services/account.service';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { User } from '../../../../interfaces/user';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { MatIconModule } from '@angular/material/icon';

interface LoginDto {
  email: string;
  password: string;
}

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    HeaderComponent,
    FooterComponent,
    MatIconModule,
    RouterLink
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private accountService = inject(AccountService);
  private router = inject(Router);
  private snackbarService = inject(SnackbarService);

  hidePassword = true;
  loginDto: LoginDto = { email: '', password: '' };

  onSubmit() {
    if (!this.loginDto.email || !this.loginDto.password) {
      alert('Please fill in all fields');
      return;
    }

    this.accountService.login(this.loginDto).subscribe({
      next: (user: User) => {
        this.accountService.setCurrentUser(user);
        this.router.navigateByUrl('/admin');
      },
      error: () => {
        this.snackbarService.error(
          'Invalid email or password. Please check your credentials'
        );
      },
    });
  }
}
