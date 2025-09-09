import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { SnackbarService } from '../../../core/services/snackbar.service';
import { MatIconModule } from '@angular/material/icon';
import { AsyncPipe, CommonModule } from '@angular/common';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { UserSessionService } from '../../../core/services/user-session.service';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-admin-header',
  imports: [
    RouterLink,
    MatIconModule,
    CommonModule,
    MatProgressBarModule,
    AsyncPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './admin-header.component.html',
  styleUrl: './admin-header.component.scss',
})
export class AdminHeaderComponent implements OnInit {
  private authenticationService = inject(AuthenticationService);
  private userSessionService = inject(UserSessionService);
  private snackBarService = inject(SnackbarService);
  private loadingService = inject(LoadingService);
  loading$ = this.loadingService.loading$;
  private router = inject(Router);

  userName = '';
  menuOpen = false; // Dropdown user menu
  mobileMenuOpen = false; // Mobile hamburger menu

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

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu() {
    this.mobileMenuOpen = false;
  }

  handleLogout() {
    this.authenticationService.logout().subscribe({
      next: () => {
        this.router.navigate(['/']);
        this.snackBarService.success("You've logged out");
        this.closeMenu();
        this.closeMobileMenu();
      },
      error: (err) => {
        this.snackBarService.error('Logout failed');
      },
    });
  }
}
