import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { RegistrationService } from '../../../../core/services/registration.service';

@Component({
  selector: 'app-generate-token-dialog',
  standalone: true,
  templateUrl: './generate-token-dialog.component.html',
  styleUrls: ['./generate-token-dialog.component.scss'],
  imports: [MatIconModule, MatDialogModule, MatButtonModule, CommonModule],
})
export class GenerateTokenDialogComponent {
  private clipboard = inject(Clipboard);
  private snackbarService = inject(SnackbarService)
  private registrationService = inject(RegistrationService)
  readonly dialogRef = inject(MatDialogRef<GenerateTokenDialogComponent>);

  inviteToken: string | null = null;

  generateToken() {
    this.registrationService.generateInviteToken().subscribe({
      next: (token) => {
        this.inviteToken = token
      },
      error: (err) => {
        this.snackbarService.error('An error has occurred')
      }
    })
  }

  copyToClipboard() {
    if (this.inviteToken) {
      this.clipboard.copy(this.inviteToken);
      this.snackbarService.success('Invite token copied!')
    }
  }

  close() {
    this.dialogRef.close();
  }
}
