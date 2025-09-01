import { Component, OnInit, inject } from '@angular/core';
import { SaintsService } from '../../core/services/saints.service';
import { Saint } from '../../features/saints/interfaces/saint';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

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
  ],
})
export class HomePageComponent implements OnInit {
  private saintsService = inject(SaintsService);

  saintsOfTheDay: Saint[] = [];
  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loading = true;
    this.saintsService.getSaintsOfTheDay().subscribe({
      next: (saints) => {
        this.saintsOfTheDay = saints;
        this.loading = false;
        console.log(this.saintsOfTheDay);
      },
      error: (err) => {
        console.error('Failed to load saints of the day:', err);
        this.error =
          'Could not load saints of the day. Please try again later.';
        this.loading = false;
      },
    });
  }
}
