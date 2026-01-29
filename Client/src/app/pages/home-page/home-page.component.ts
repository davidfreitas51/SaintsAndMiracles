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
import { SaintFilters } from '../../features/saints/interfaces/saint-filter';
import { Prayer } from '../../features/prayers/interfaces/prayer';
import { Miracle } from '../../features/miracles/interfaces/miracle';

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
  universalFeastsOfTheDay: Saint[] = []
  saintsOfThisMonth: Saint[] = [];
  recentPrayers: Prayer[] = [];
  recentMiracles: Miracle[] = [];
  currentMonth!: string;

  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    const now = new Date();
    this.currentMonth = now.toLocaleString('en-US', { month: 'long' });
    this.loadSaintOfTheDay();
    this.loadSaintsOfThisMonth();
  }

  private loadSaintOfTheDay() {
    this.saintsService.getSaintsOfTheDay().subscribe({
      next: (saint) => {
        this.universalFeastsOfTheDay = saint;
      },
      error: (err) => {
        console.error('Failed to load universal feast saint:', err);
      },
    });
  }

  private loadSaintsOfThisMonth() {
    const filters = new SaintFilters();
    filters.feastMonth = (new Date().getMonth() + 1).toString();
    filters.orderBy = 'feastDay';


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
