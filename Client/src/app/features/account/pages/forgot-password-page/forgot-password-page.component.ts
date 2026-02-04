import { Component, inject } from '@angular/core';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { RouterLink } from '@angular/router';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { safeEmailValidator } from '../../../../shared/validators/safe-email.validator';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-forgot-password-page',
  imports: [
    FooterComponent,
    HeaderComponent,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule,
    RouterLink,
    ReactiveFormsModule,
  ],
  templateUrl: './forgot-password-page.component.html',
  styleUrl: './forgot-password-page.component.scss',
})
export class ForgotPasswordPageComponent {
  private fb = inject(FormBuilder);
  private accountManagementService = inject(AccountManagementService);
  private snackBarService = inject(SnackbarService);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, safeEmailValidator]],
  });

  isSubmitting = false;

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const email = this.form.value.email!;

    this.isSubmitting = true;

    this.accountManagementService
      .requestPasswordReset(email)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.snackBarService.success(
            'If an account exists with that email, a reset link was sent.',
          );
        },
        error: () => {
          this.snackBarService.error(
            'Something went wrong. Please try again later.',
          );
        },
      });
  }
}
