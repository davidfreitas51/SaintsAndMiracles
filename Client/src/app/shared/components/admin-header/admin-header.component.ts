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
  private router = inject(Router);

  loading$ = this.loadingService.loading$;

  userName = '';
  menuOpen = false;
  mobileMenuOpen = false;
  isDarkMode = false;

  ngOnInit(): void {
    this.userSessionService.currentUser$.subscribe({
      next: (user) => {
        if (user) {
          this.userName = `${user.firstName} ${user.lastName}`;
        }
      },
    });

    this.applyDarkModePreference();
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

  toggleDarkMode() {
    const htmlEl = document.documentElement;
    htmlEl.classList.toggle('dark');
    this.isDarkMode = htmlEl.classList.contains('dark');
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
  }

  applyDarkModePreference() {
    const htmlEl = document.documentElement;
    const storedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia(
      '(prefers-color-scheme: dark)',
    ).matches;

    if (storedTheme === 'dark' || (!storedTheme && prefersDark)) {
      htmlEl.classList.add('dark');
      this.isDarkMode = true;
    } else {
      htmlEl.classList.remove('dark');
      this.isDarkMode = false;
    }
  }

  handleLogout() {
    this.authenticationService.logout().subscribe({
      next: () => {
        this.router.navigate(['/']);
        this.snackBarService.success("You've logged out");
        this.closeMenu();
        this.closeMobileMenu();
      },
      error: () => {
        this.snackBarService.error('Logout failed');
      },
    });
  }
}
