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
  universalFeastOfTheDay: Saint | null = null;
  saintsOfThisMonth: Saint[] = [];
  loading = false;

  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    this.loadUniversalFeastOfTheDay();
    this.loadSaintsOfThisMonth();
  }

  private loadUniversalFeastOfTheDay() {
    this.loading = true;
    this.saintsService.getSaintOfTheDay().subscribe({
      next: (saint) => {
        this.universalFeastOfTheDay = saint;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load universal feast saint:', err);
        this.loading = false;
      },
    });
  }

  private loadSaintsOfThisMonth() {
    const filters = new SaintFilters();
    filters.feastMonth = (new Date().getMonth() + 1).toString();
    filters.orderBy = 'feastDay';

    this.saintsService.getSaints(filters).subscribe({
      next: (response) => {
        this.saintsOfThisMonth = this.universalFeastOfTheDay
          ? response.items.filter(
              (s) => s.id !== this.universalFeastOfTheDay?.id
            )
          : response.items;
      },
      error: (err) => {
        console.error('Failed to load saints of the month:', err);
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
