import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MiraclesService } from '../../../../core/services/miracles.service';
import { environment } from '../../../../../environments/environment';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatDividerModule } from '@angular/material/divider';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import countries from 'i18n-iso-countries';
import { marked } from 'marked';
import enLocale from 'i18n-iso-countries/langs/en.json';
import { CommonModule } from '@angular/common';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
countries.registerLocale(enLocale);

@Component({
  selector: 'app-miracle-details-page',
  templateUrl: './miracle-details-page.component.html',
  imports: [
    FooterComponent,
    HeaderComponent,
    MatDividerModule,
    RomanPipe,
    CommonModule,
    CountryCodePipe,
    RouterLink,
  ],
})
export class MiracleDetailsPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private miraclesService = inject(MiraclesService);
  private sanitizer = inject(DomSanitizer);
  imageBaseUrl = environment.assetsUrl;
  public miracle: any = null;
  markdownContent!: SafeHtml;
  headings: { id: string; text: string }[] = [];

  ngOnInit(): void {
    const slug =
      this.route.snapshot.url
        .map((s) => s.path)
        .join('/')
        .split('/')
        .pop() || '';
    this.miraclesService.getMiracleWithMarkdown(slug).subscribe({
      next: async (data) => {
        this.miracle = data.miracle;
        this.headings = [];

        const renderer = new marked.Renderer();

        renderer.heading = ({ depth, text }) => {
          if (depth === 2) {
            const id = text.toLowerCase().replace(/[^\w]+/g, '-');
            this.headings.push({ id, text });
            return `<h2 id="${id}" class="scroll-mt-32 text-xl font-semibold mt-8 mb-2">${text}</h2>`;
          }
          return `<h${depth}>${text}</h${depth}>`;
        };

        const rawHtml = await marked.parse(data.markdown, { renderer });
        this.markdownContent = this.sanitizer.bypassSecurityTrustHtml(rawHtml);
      },
      error: (err) => {
        console.error('Error loading miracle: ', err);
      },
    });
  }

  scrollTo(id: string): void {
    const el = document.getElementById(id);
    if (!el) return;

    const headerHeight = 120; 
    const targetY =
      el.getBoundingClientRect().top + window.scrollY - headerHeight;
    const startY = window.scrollY;
    const distance = targetY - startY;
    const duration = 200;
    let startTime: number | null = null;

    const animate = (time: number) => {
      if (!startTime) startTime = time;
      const elapsed = time - startTime;
      const progress = Math.min(elapsed / duration, 1);
      window.scrollTo(0, startY + distance * progress);
      if (progress < 1) requestAnimationFrame(animate);
    };

    requestAnimationFrame(animate);
  }
}
