import {
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { SaintsService } from '../../core/services/saints.service';
import { Saint } from '../../features/saints/interfaces/saint';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RomanPipe } from '../../shared/pipes/roman.pipe';
import { CountryCodePipe } from '../../shared/pipes/country-code.pipe';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../environments/environment';
import { Prayer } from '../../features/prayers/interfaces/prayer';
import { Miracle } from '../../features/miracles/interfaces/miracle';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
  imports: [
    HeaderComponent,
    FooterComponent,
    RouterLink,
    CommonModule,
    MatCardModule,
    MatButtonModule,
    RomanPipe,
    CountryCodePipe,
    MatIconModule,
  ],
})
export class HomePageComponent implements OnInit {
  private saintsService = inject(SaintsService);
  private router = inject(Router);

  imageBaseUrl = environment.assetsUrl;

  universalFeastsOfTheDay: Saint[] = [];
  upcomingFeasts: Saint[] = [];
  recentPrayers: Prayer[] = [];
  recentMiracles: Miracle[] = [];

  loadingSaintsOfTheDay = true;
  loadingUpcomingFeasts = true;

  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    this.loadSaintsOfTheDay();
    this.loadUpcomingFeasts();
  }

  private loadSaintsOfTheDay() {
    this.saintsService
      .getSaintsOfTheDay()
      .pipe(
        finalize(() => {
          this.loadingSaintsOfTheDay = false;
        }),
      )
      .subscribe({
        next: (saints) => {
          this.universalFeastsOfTheDay = saints ?? [];
        },
        error: (err) => {
          console.error('Failed to load saints of the day:', err);
          this.universalFeastsOfTheDay = [];
        },
      });
  }

  private loadUpcomingFeasts() {
    this.saintsService
      .getUpcomingSaints()
      .pipe(
        finalize(() => {
          this.loadingUpcomingFeasts = false;
        }),
      )
      .subscribe({
        next: (feasts) => {
          this.upcomingFeasts = feasts ?? [];
        },
        error: (err) => {
          console.error('Failed to load upcoming feasts:', err);
          this.upcomingFeasts = [];
        },
      });
  }

  scroll(direction: 'left' | 'right') {
    const container = this.scrollContainer.nativeElement;
    const scrollAmount = 300;

    container.scrollBy({
      left: direction === 'left' ? -scrollAmount : scrollAmount,
      behavior: 'smooth',
    });
  }

  goToSaint(slug: string) {
    this.router.navigate(['/saints', slug]);
  }
}
