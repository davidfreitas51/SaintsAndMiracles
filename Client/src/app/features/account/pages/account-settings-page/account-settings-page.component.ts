import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminHeaderComponent } from '../../../../shared/components/admin-header/admin-header.component';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AccountManagementService } from '../../../../core/services/account-management.service';
import { UserSessionService } from '../../../../core/services/user-session.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { matchValueValidator } from '../../../../shared/validators/match-value.validator';
import { passwordValidator } from '../../../../shared/validators/password.validator';
import { personNameValidator } from '../../../../shared/validators/person-name.validator';

@Component({
  selector: 'app-account-settings-page',
  templateUrl: './account-settings-page.component.html',
  styleUrls: ['./account-settings-page.component.scss'],
  standalone: true,
  imports: [
    AdminHeaderComponent,
    MatCardModule,
    MatInputModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
  ],
})
export class AccountSettingsPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private accountManagementService = inject(AccountManagementService);
  private userSessionService = inject(UserSessionService);
  private snackbarService = inject(SnackbarService);
  private router = inject(Router);

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  isProfileSubmitting = false;
  isEmailSubmitting = false;
  isPasswordSubmitting = false;

  profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, personNameValidator]],
    lastName: ['', [Validators.required, personNameValidator]],
  });

  emailForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  passwordForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, passwordValidator]],
      confirmPassword: ['', [Validators.required]],
    },
    {
      validators: matchValueValidator('newPassword', 'confirmPassword'),
    },
  );

  ngOnInit(): void {
    this.userSessionService.currentUser$.subscribe((user) => {
      if (!user) return;

      this.profileForm.patchValue({
        firstName: user.firstName,
        lastName: user.lastName,
      });

      this.emailForm.patchValue({
        email: user.email,
      });
    });

    this.passwordForm.controls.newPassword.valueChanges.subscribe(() => {
      this.passwordForm.controls.confirmPassword.updateValueAndValidity();
    });
  }

  updateProfile() {
    if (this.profileForm.invalid || this.isProfileSubmitting) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const dto = {
      firstName: this.profileForm.controls.firstName.value,
      lastName: this.profileForm.controls.lastName.value,
    };

    this.isProfileSubmitting = true;

    this.accountManagementService
      .updateProfile(dto)
      .pipe(finalize(() => (this.isProfileSubmitting = false)))
      .subscribe({
        next: (user) => {
          this.snackbarService.success('Profile updated successfully!');
          this.userSessionService.setUser(user);
        },
        error: (err) => {
          this.snackbarService.error(
            err?.error?.message || 'Failed to update profile.',
          );
        },
      });
  }

  updateEmail() {
    if (this.emailForm.invalid || this.isEmailSubmitting) {
      this.emailForm.markAllAsTouched();
      return;
    }

    const email = this.emailForm.controls.email.value;

    this.isEmailSubmitting = true;

    this.accountManagementService
      .requestEmailChange(email)
      .pipe(finalize(() => (this.isEmailSubmitting = false)))
      .subscribe({
        next: () =>
          this.snackbarService.success(
            'Confirmation email sent! Check your inbox.',
          ),
        error: (err) => {
          this.snackbarService.error(
            err?.error?.message || 'Failed to send confirmation email.',
          );
        },
      });
  }

  updatePassword() {
    if (this.passwordForm.invalid || this.isPasswordSubmitting) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const dto = {
      currentPassword: this.passwordForm.controls.currentPassword.value,
      newPassword: this.passwordForm.controls.newPassword.value,
    };

    this.isPasswordSubmitting = true;

    this.accountManagementService
      .changePassword(dto)
      .pipe(finalize(() => (this.isPasswordSubmitting = false)))
      .subscribe({
        next: () => {
          this.snackbarService.success('Password changed successfully.');
          this.passwordForm.reset();
          this.router.navigateByUrl('/account/login');
        },
        error: (err) => {
          const message =
            err?.error?.message ||
            err?.error?.Errors?.join(', ') ||
            'Failed to change password.';

          this.snackbarService.error(message);
        },
      });
  }
}
