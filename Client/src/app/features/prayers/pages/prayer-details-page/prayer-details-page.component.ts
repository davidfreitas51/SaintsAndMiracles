import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatDividerModule } from '@angular/material/divider';
import { marked } from 'marked';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { PrayersService } from '../../../../core/services/prayers.service';

@Component({
  selector: 'app-prayer-details-page',
  templateUrl: './prayer-details-page.component.html',
  imports: [FooterComponent, HeaderComponent, MatDividerModule, RouterLink],
})
export class PrayerDetailsPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private prayersService = inject(PrayersService);
  private sanitizer = inject(DomSanitizer);

  imageBaseUrl = environment.assetsUrl;
  public prayer: any = null;
  markdownContent!: SafeHtml;
  headings: { id: string; text: string }[] = [];

  ngOnInit(): void {
    const slug =
      this.route.snapshot.url
        .map((s) => s.path)
        .join('/')
        .split('/')
        .pop() || '';

    this.prayersService.getPrayerWithMarkdown(slug).subscribe({
      next: async (data) => {
        this.prayer = data.prayer;
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
        console.error('Error loading prayer: ', err);
      },
    });
  }

  scrollTo(id: string): void {
    const el = document.getElementById(id);
    if (!el) return;

    const headerHeight = 120;
    const targetY = el.offsetTop - headerHeight;

    let scrollable: HTMLElement | Window = window;
    let parent: HTMLElement | null = el.parentElement;
    while (parent) {
      const style = getComputedStyle(parent);
      if (style.overflowY === 'auto' || style.overflowY === 'scroll') {
        scrollable = parent;
        break;
      }
      parent = parent.parentElement;
    }

    if (scrollable instanceof Window) {
      window.scrollTo({ top: targetY, behavior: 'smooth' });
    } else {
      scrollable.scrollTo({ top: targetY, behavior: 'smooth' });
    }
  }
}
