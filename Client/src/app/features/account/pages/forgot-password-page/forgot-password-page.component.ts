import { Component, inject } from '@angular/core';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';

@Component({
  selector: 'app-forgot-password-page',
  imports: [
    FooterComponent,
    HeaderComponent,
    MatCardModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    CommonModule,
    RouterLink,
  ],
  templateUrl: './forgot-password-page.component.html',
  styleUrl: './forgot-password-page.component.scss',
})
export class ForgotPasswordPageComponent {
  private accountManagementService = inject(AccountManagementService);
  private snackBarService = inject(SnackbarService);
  forgotPasswordDto = { email: '' };

  onSubmit() {
    if (!this.forgotPasswordDto.email) return;
    this.accountManagementService
      .requestPasswordReset(this.forgotPasswordDto.email)
      .subscribe({
        next: () => {
          this.snackBarService.success(
            'If an account exists with that email, a reset link was sent.'
          );
        },
        error: (err) => {
          this.snackBarService.error('Something went wrong. Please try again later.');
        },
      });
  }
}
