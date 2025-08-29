import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminContentTableComponent } from '../../../../shared/components/admin-content-table/admin-content-table.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EntityFilters } from '../../../../interfaces/entity-filters';
import { User } from '../../../../interfaces/user';
import { GenerateTokenDialogComponent } from '../../components/generate-token-dialog/generate-token-dialog.component';
import { AccountManagementService } from '../../../../core/services/account-management.service';

@Component({
  selector: 'app-manage-accounts-page',
  standalone: true,
  imports: [
    AdminContentTableComponent,
    MatIconModule,
    MatButtonModule,
    RouterLink,
    MatButtonModule,
  ],
  templateUrl: './manage-accounts-page.component.html',
  styleUrls: ['./manage-accounts-page.component.scss'],
})
export class ManageAccountsPageComponent {
  private accountManagementService = inject (AccountManagementService)
  private dialog = inject(MatDialog);

  entityFilter: EntityFilters = new EntityFilters();

  accounts: User[] = [];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers = () => {
    //this.usersService.getUsers().subscribe(() => {
    //this.users = res.items;
    //});
  };

  deleteUser = (id: number) => this.accountManagementService.deleteUser(id);

  openRegisterAccountDialog() {
    this.dialog.open(GenerateTokenDialogComponent, {
      height: '500px',
      width: '600px',
      panelClass: 'generate-token-dialog',
    });
  }
}
