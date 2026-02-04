import { Component, inject } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterLink } from '@angular/router';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { MatIconModule } from '@angular/material/icon';
import { LoginDto } from '../../interfaces/login-dto';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthenticationService } from '../../../../core/services/authentication.service';
import { RegistrationService } from '../../../../core/services/registration.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { safeEmailValidator } from '../../../../shared/validators/safe-email.validator';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
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
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authenticationService = inject(AuthenticationService);
  private registrationService = inject(RegistrationService);
  private snackbarService = inject(SnackbarService);

  hidePassword = true;
  isSubmitting = false;
  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, safeEmailValidator]],
    password: ['', Validators.required],
    rememberMe: false,
  });

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const dto: LoginDto = {
      email: this.form.controls.email.value,
      password: this.form.controls.password.value,
      rememberMe: this.form.controls.rememberMe.value,
    };

    this.authenticationService
      .login(dto)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.router.navigate(['/admin']);
        },
        error: (err) => {
          const msg = err?.error || err?.message || 'Invalid credentials';

          if (msg.includes('Email not confirmed')) {
            this.registrationService
              .resendConfirmation(dto.email)
              .subscribe(() => {
                this.snackbarService.error(
                  'Your email is not confirmed. A new confirmation email has been sent.',
                );
              });
          } else {
            this.snackbarService.error(msg);
          }
        },
      });
  }
}
