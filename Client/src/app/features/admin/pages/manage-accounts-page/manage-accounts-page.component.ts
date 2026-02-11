import { Component, inject } from '@angular/core';
import { AdminContentTableComponent } from '../../../../shared/components/admin-content-table/admin-content-table.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EntityFilters } from '../../../../interfaces/entity-filters';
import { GenerateTokenDialogComponent } from '../../components/generate-token-dialog/generate-token-dialog.component';
import { AccountManagementService } from '../../../../core/services/account-management.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { UserSummary } from '../../../account/interfaces/user-summary';

@Component({
  selector: 'app-manage-accounts-page',
  standalone: true,
  imports: [AdminContentTableComponent, MatIconModule, MatButtonModule],
  templateUrl: './manage-accounts-page.component.html',
  styleUrls: ['./manage-accounts-page.component.scss'],
})
export class ManageAccountsPageComponent {
  private accountManagementService = inject(AccountManagementService);
  private snackbarService = inject(SnackbarService);
  private dialog = inject(MatDialog);

  entityFilter: EntityFilters = new EntityFilters();

  accounts: UserSummary[] = [];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers = () => {
    this.accountManagementService.getAllUsers().subscribe({
      next: (users) => (this.accounts = users),
      error: (err) => this.snackbarService.error('Error loading accounts'),
    });
  };

  deleteUser = (email: string) =>
    this.accountManagementService.deleteUser(email);

  openRegisterAccountDialog() {
    this.dialog.open(GenerateTokenDialogComponent, {
      height: '750px',
      width: '600px',
      panelClass: 'generate-token-dialog',
    });
  }
}
