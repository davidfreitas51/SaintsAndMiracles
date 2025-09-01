import { Component, OnInit, inject } from '@angular/core';
import { SaintsService } from '../../core/services/saints.service';
import { Saint } from '../../features/saints/interfaces/saint';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';


@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
  imports: [HeaderComponent, FooterComponent]
})
export class LandingPageComponent implements OnInit {
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
        console.log(this.saintsOfTheDay)
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
