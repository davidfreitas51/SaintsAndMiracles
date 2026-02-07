import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  NgForm,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { RegisterDto } from '../../interfaces/register-dto';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { RegistrationService } from '../../../../core/services/registration.service';
import { safeEmailValidator } from '../../../../shared/validators/safe-email.validator';
import { personNameValidator } from '../../../../shared/validators/person-name.validator';
import { matchValueValidator } from '../../../../shared/validators/match-value.validator';
import { finalize } from 'rxjs';
import { passwordValidator } from '../../../../shared/validators/password.validator';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FooterComponent,
    HeaderComponent,
    RouterLink,
    MatIconModule,
  ],
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.scss'],
})
export class RegisterPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private snackbarService = inject(SnackbarService);
  private registrationService = inject(RegistrationService);
  private router = inject(Router);

  backendErrors: string[] = [];
  hidePassword = true;
  hideConfirmPassword = true;
  isSubmitting = false;

  form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, personNameValidator]],
      lastName: ['', [Validators.required, personNameValidator]],
      email: ['', [Validators.required, Validators.email, safeEmailValidator]],
      password: ['', [Validators.required, passwordValidator]],
      confirmPassword: ['', [Validators.required]],
      token: ['', [Validators.required]],
    },
    {
      validators: matchValueValidator('password', 'confirmPassword'),
    },
  );

  ngOnInit() {
    this.form.controls.password.valueChanges.subscribe(() => {
      this.form.controls.confirmPassword.updateValueAndValidity();
    });
  }

  onSubmit() {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    const dto: RegisterDto = {
      firstName: this.form.controls.firstName.value,
      lastName: this.form.controls.lastName.value,
      email: this.form.controls.email.value,
      password: this.form.controls.password.value,
      confirmPassword: this.form.controls.confirmPassword.value,
      inviteToken: this.form.controls.token.value,
    };

    this.isSubmitting = true;

    this.registrationService
      .register(dto)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.snackbarService.success(
            'Registration successful! Please confirm your email before login',
          );

          this.router.navigateByUrl('/account/registration-confirmation');
        },

        error: (err) => {
          const details: string[] = err?.error?.details || [];

          if (details.length > 0) {
            details.forEach((d) => this.snackbarService.error(d));
          } else {
            this.snackbarService.error(
              err?.error?.message ||
                'Registration failed. Please check your inputs.',
            );
          }
        },
      });
  }
}
