import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { SnackbarService } from '../../../core/services/snackbar.service';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { UserSessionService } from '../../../core/services/user-session.service';

@Component({
  selector: 'app-admin-header',
  imports: [RouterLink, MatIconModule, CommonModule],
  templateUrl: './admin-header.component.html',
  styleUrl: './admin-header.component.scss',
})
export class AdminHeaderComponent implements OnInit {
  private authenticationService = inject(AuthenticationService)
  private userSessionService = inject(UserSessionService)
  private snackBarService = inject(SnackbarService);
  private router = inject(Router);

  userName = '';
  menuOpen = false;

  ngOnInit(): void {
    this.userSessionService.currentUser$.subscribe({
      next: (user) => {
        if (user) {
          this.userName = `${user.firstName} ${user.lastName}`;
        }
      },
    });
  }

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu() {
    this.menuOpen = false;
  }

  handleLogout() {
    this.authenticationService.logout().subscribe({
      next: () => {
        this.router.navigate(['/']);
        this.snackBarService.success("You've logged out");
      },
      error: (err) => {
        this.snackBarService.error('Logout failed');
      },
    });
  }
}
