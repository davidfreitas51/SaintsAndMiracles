import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [
    HeaderComponent,
    FooterComponent,
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './reset-password-page.component.html',
  styleUrls: ['./reset-password-page.component.scss'],
})
export class ResetPasswordPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private accountManagementService = inject(AccountManagementService);
  private snackBar = inject(SnackbarService);
  private router = inject(Router);

  resetPasswordDto = {
    email: '',
    token: '',
    newPassword: '',
    confirmPassword: '',
  };

  hidePassword = true;
  hideConfirmPassword = true;

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.resetPasswordDto.email = params['email'] || '';
      this.resetPasswordDto.token = params['token'] || '';
    });
  }

  onSubmit(form?: NgForm) {
    if (!form?.valid) return;
    if (
      this.resetPasswordDto.newPassword !==
      this.resetPasswordDto.confirmPassword
    ) {
      this.snackBar.error('Passwords do not match.');
      return;
    }

    this.accountManagementService.resetPassword(this.resetPasswordDto).subscribe({
      next: () => {
        this.snackBar.success('Password successfully reset.');
        this.router.navigate(['/account/login']);
      },
      error: (err) => {
        this.snackBar.error('Failed to reset password. Please try again.');
      },
    });
  }
}
