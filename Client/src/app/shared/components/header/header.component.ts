import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  imports: [
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatMenuModule,
    MatProgressBarModule,
    CommonModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent implements OnInit {
  private loadingService = inject(LoadingService);
  loading$ = this.loadingService.loading$;

  isDarkMode = false; // controla o estado atual do tema

  ngOnInit(): void {
    this.applyDarkModePreference();
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
      '(prefers-color-scheme: dark)'
    ).matches;

    if (storedTheme === 'dark' || (!storedTheme && prefersDark)) {
      htmlEl.classList.add('dark');
      this.isDarkMode = true;
    } else {
      htmlEl.classList.remove('dark');
      this.isDarkMode = false;
    }
  }
}
