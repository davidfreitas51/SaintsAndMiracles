import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterLink } from '@angular/router';
import { AccountsService } from '../../../../core/services/accounts.service';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { User } from '../../../../interfaces/user';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { MatIconModule } from '@angular/material/icon';
import { LoginDto } from '../../interfaces/login-dto';
import { MatCheckboxModule } from '@angular/material/checkbox';

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
    RouterLink,
    MatCheckboxModule,
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private router = inject(Router);
  private accountsService = inject(AccountsService);
  private snackbarService = inject(SnackbarService);

  hidePassword = true;
  loginDto: LoginDto = { email: '', password: '', rememberMe: false };

  onSubmit() {
    if (!this.loginDto.email || !this.loginDto.password) return;

    this.accountsService.login(this.loginDto).subscribe({
      next: (user: User) => {
        this.snackbarService.success('Login successful');
        this.router.navigate(['/admin']);
      },
      error: (err) => {
        const msg = err?.error || err?.message || 'Invalid credentials';

        if (msg.includes('Email not confirmed')) {
          this.accountsService
            .resendConfirmation(this.loginDto.email)
            .subscribe(() => {
              this.snackbarService.error(
                'Your email is not confirmed. A new confirmation email has been sent.'
              );
            });
        } else {
          this.snackbarService.error(msg);
        }
      },
    });
  }
}
