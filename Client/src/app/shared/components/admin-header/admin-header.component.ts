import { Component, inject } from '@angular/core';
import { AccountsService } from '../../../core/services/accounts.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-header',
  imports: [
    RouterLink
  ],
  templateUrl: './admin-header.component.html',
  styleUrl: './admin-header.component.scss',
})
export class AdminHeaderComponent {
  private accountService = inject(AccountsService);
  private router = inject(Router);

  handleLogout() {
    this.accountService.logout().subscribe({
      next: () => {
        this.router.navigate(['/']); 
      },
      error: (err) => {
        console.error('Logout failed', err);
      },
    });
  }
}
