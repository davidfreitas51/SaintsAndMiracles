import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AdminHeaderComponent } from '../../../../shared/components/admin-header/admin-header.component';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { UserSessionService } from '../../../../core/services/user-session.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-account-settings-page',
  templateUrl: './account-settings-page.component.html',
  styleUrls: ['./account-settings-page.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    AdminHeaderComponent,
    MatCardModule,
    MatInputModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
  ],
})
export class AccountSettingsPageComponent implements OnInit {
  public profileForm!: FormGroup;
  public emailForm!: FormGroup;
  public passwordForm!: FormGroup;

  private accountService = inject(AccountManagementService);
  private userSessionService = inject(UserSessionService);
  private snackBarService = inject(SnackbarService);
  private fb = inject(FormBuilder);

  ngOnInit(): void {
    this.userSessionService.currentUser$.subscribe((user) => {
      if (user) {
        this.profileForm = this.fb.group({
          firstName: [user.firstName, Validators.required],
          lastName: [user.lastName, Validators.required],
        });

        this.emailForm = this.fb.group({
          email: [user.email, [Validators.required, Validators.email]],
        });
      }
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
    });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) return;

    this.accountService.updateProfile(this.profileForm.value).subscribe({
      next: (user) => {
        this.snackBarService.success('Profile updated successfully!');
        this.userSessionService.setUser(user);
      },
      error: (err) => {
        console.error(err);
        this.snackBarService.error('Failed to update profile.');
      },
    });
  }

  updateEmail(): void {
    if (this.emailForm.invalid) return;
  }

  updatePassword(): void {
    if (this.passwordForm.invalid) return;
  }
}
