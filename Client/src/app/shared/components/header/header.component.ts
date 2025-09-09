import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { RouterLink, RouterLinkActive, RouterModule } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingService } from '../../../core/services/loading.service';
import { AsyncPipe, CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [
    MatToolbarModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    MatDividerModule,
    RouterLink,
    RouterModule,
    RouterLinkActive,
    MatProgressBarModule,
    CommonModule,
    AsyncPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit {
  private loadingService = inject(LoadingService);
  loading$ = this.loadingService.loading$;

  ngOnInit(): void {
    this.applyDarkModePreference();
  }

  toggleDarkMode() {
    const htmlEl = document.documentElement;
    htmlEl.classList.toggle('dark');

    if (htmlEl.classList.contains('dark')) {
      localStorage.setItem('theme', 'dark');
    } else {
      localStorage.setItem('theme', 'light');
    }
  }

  applyDarkModePreference() {
    const htmlEl = document.documentElement;
    const storedTheme = localStorage.getItem('theme');

    if (
      storedTheme === 'dark' ||
      (!storedTheme &&
        window.matchMedia('(prefers-color-scheme: dark)').matches)
    ) {
      htmlEl.classList.add('dark');
    } else {
      htmlEl.classList.remove('dark');
    }
  }
}
