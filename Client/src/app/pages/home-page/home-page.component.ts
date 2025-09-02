import { Component, OnInit, inject } from '@angular/core';
import { SaintsService } from '../../core/services/saints.service';
import { Saint } from '../../features/saints/interfaces/saint';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RomanPipe } from "../../shared/pipes/roman.pipe";
import { environment } from '../../../environments/environment';
import { CountryCodePipe } from "../../shared/pipes/country-code.pipe";

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
    CountryCodePipe
],
})
export class HomePageComponent implements OnInit {
  private saintsService = inject(SaintsService);
  imageBaseUrl = environment.assetsUrl;

  universalFeastOfTheDay: Saint | null = null;
  loading = false;

  ngOnInit(): void {
    this.loading = true;
    this.saintsService.getUniversalFeastOfTheDay().subscribe({
      next: (saint) => {
        this.universalFeastOfTheDay = saint;
        this.loading = false;
        console.log(this.universalFeastOfTheDay);
      },
      error: (err) => {
        console.error('Failed to load universal feast saint:', err);
        this.loading = false;
      },
    });
  }
}
