import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { passwordValidator } from '../../../../shared/validators/password.validator';
import { matchValueValidator } from '../../../../shared/validators/match-value.validator';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    HeaderComponent,
    FooterComponent,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    RouterLink,
  ],
  templateUrl: './reset-password-page.component.html',
  styleUrls: ['./reset-password-page.component.scss'],
})
export class ResetPasswordPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private accountManagementService = inject(AccountManagementService);
  private snackBarService = inject(SnackbarService);
  private router = inject(Router);

  hidePassword = true;
  hideConfirmPassword = true;
  isSubmitting = false;

  form = this.fb.nonNullable.group(
    {
      password: ['', [Validators.required, passwordValidator]],
      confirmPassword: ['', Validators.required],
      email: [''],
      token: [''],
    },
    {
      validators: matchValueValidator('password', 'confirmPassword'),
    },
  );

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.form.patchValue({
        email: params['email'] || '',
        token: params['token'] || '',
      });
    });

    this.form.controls.password.valueChanges.subscribe(() => {
      this.form.controls.confirmPassword.updateValueAndValidity();
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { password, confirmPassword, email, token } = this.form.value;

    this.isSubmitting = true;

    this.accountManagementService
      .resetPassword({
        email: email!,
        token: token!,
        newPassword: password!,
        confirmPassword: confirmPassword!,
      })
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.snackBarService.success('Password successfully reset.');
          this.router.navigate(['/account/login']);
        },
        error: (err) => {
          if (err.error?.Errors?.length) {
            this.snackBarService.error(err.error.Errors.join(' '));
          } else {
            this.snackBarService.error(
              'Failed to reset password. Please try again.',
            );
          }
        },
      });
  }
}
