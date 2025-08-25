import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { RegisterDto } from '../../interfaces/register-dto';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../../../core/services/account.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FooterComponent,
    HeaderComponent,
    RouterLink,
    MatIconModule,
    CommonModule,
  ],
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.scss'],
})
export class RegisterPageComponent {
  private accountService = inject(AccountService);
  private snackbarService = inject(SnackbarService)
  private router = inject(Router)

  hidePassword = true;
  hideConfirmPassword = true;

  registerDto: RegisterDto = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    inviteToken: '',
  };

  onSubmit(form: NgForm) {
    form.form.markAllAsTouched(); 
    form.form.updateValueAndValidity();

    if (form.invalid) return;

    if (this.registerDto.password !== this.registerDto.confirmPassword) {
      this.snackbarService.error('Passwords do not match!');
      return;
    }

    this.accountService.register(this.registerDto).subscribe({
      next: () => {
        this.snackbarService.success(
          'Registration successful! Please confirm your email before login'
        );
        this.router.navigateByUrl('/account/login');
      },
      error: (err) => {
        this.snackbarService.error(
          err?.error?.message ||
            'Registration failed. Please check your inputs.'
        );
      },
    });
  }
}
