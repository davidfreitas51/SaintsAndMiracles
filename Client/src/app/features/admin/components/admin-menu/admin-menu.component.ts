import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { UserSessionService } from '../../../../core/services/user-session.service';

@Component({
  selector: 'app-admin-menu',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './admin-menu.component.html',
  styleUrls: ['./admin-menu.component.scss'],
})
export class AdminMenuComponent implements OnInit {
  private userSessionService = inject(UserSessionService);
  public isSuperAdmin = false;

  ngOnInit(): void {
    this.userSessionService.userRole$.subscribe({
      next: (role) => {
        this.isSuperAdmin = role === 'SuperAdmin';
      },
      error: () => {
        this.isSuperAdmin = false;
      },
    });

    if (!this.userSessionService.getUserRole()) {
      this.userSessionService.fetchUserRole();
    }
  }
}
