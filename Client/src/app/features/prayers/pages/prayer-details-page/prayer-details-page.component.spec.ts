import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

import { PrayerDetailsPageComponent } from './prayer-details-page.component';
import { PrayersService } from '../../../../core/services/prayers.service';
import { environment } from '../../../../../environments/environment';

describe('PrayerDetailsPageComponent', () => {
  let component: PrayerDetailsPageComponent;
  let fixture: ComponentFixture<PrayerDetailsPageComponent>;

  let prayersService: {
    getPrayerWithMarkdown: ReturnType<typeof vi.fn>;
  };

  let activatedRoute: {
    snapshot: {
      url: { path: string }[];
    };
  };

  const mockPrayer = {
    id: 1,
    name: 'Our Father',
    slug: 'our-father',
  };

  const mockResponse = {
    prayer: mockPrayer,
    markdown: '# Our Father\n\n## History\n\nPrayer description.',
  };

  const createRoute = (paths: string[] = ['prayers', 'our-father']) => ({
    snapshot: {
      url: paths.map((path) => ({ path })),
    },
  });

  const mockElementScrollTo = (element: HTMLElement) => {
    const scrollTo = vi.fn();

    Object.defineProperty(element, 'scrollTo', {
      configurable: true,
      writable: true,
      value: scrollTo,
    });

    return scrollTo;
  };

  beforeEach(async () => {
    Object.defineProperty(window, 'scrollTo', {
      configurable: true,
      writable: true,
      value: vi.fn(),
    });

    activatedRoute = createRoute();

    prayersService = {
      getPrayerWithMarkdown: vi.fn().mockReturnValue(of(mockResponse)),
    };

    await TestBed.configureTestingModule({
      imports: [PrayerDetailsPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        {
          provide: PrayersService,
          useValue: prayersService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PrayerDetailsPageComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    fixture?.destroy();
    vi.restoreAllMocks();
    document.body.innerHTML = '';
  });

  describe('initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.prayer).toBeNull();
      expect(component.markdownContent).toBe('');
      expect(component.headings).toEqual([]);
    });

    it('should initialize imageBaseUrl from environment', () => {
      expect(component.imageBaseUrl).toBe(environment.assetsUrl);
    });
  });

  describe('ngOnInit', () => {
    it('should request the prayer using the route slug', () => {
      fixture.detectChanges();

      expect(prayersService.getPrayerWithMarkdown).toHaveBeenCalledOnce();

      expect(prayersService.getPrayerWithMarkdown).toHaveBeenCalledWith(
        'our-father',
      );
    });

    it('should extract the last segment from a nested route', async () => {
      activatedRoute.snapshot.url = [
        { path: 'prayers' },
        { path: 'category' },
        { path: 'rosary' },
      ];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(prayersService.getPrayerWithMarkdown).toHaveBeenCalledWith(
        'rosary',
      );
    });

    it('should use an empty slug when the route has no segments', async () => {
      activatedRoute.snapshot.url = [];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(prayersService.getPrayerWithMarkdown).toHaveBeenCalledWith('');
    });

    it('should assign the returned prayer', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.prayer).toEqual(mockPrayer);
    });

    it('should clear previous headings before processing markdown', async () => {
      component.headings = [
        { id: 'previous-heading', text: 'Previous heading' },
      ];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).not.toContainEqual({
        id: 'previous-heading',
        text: 'Previous heading',
      });
    });

    it('should convert markdown into HTML', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h1>');
      expect(component.markdownContent).toContain('Our Father');
      expect(component.markdownContent).toContain('Prayer description.');
    });

    it('should log an error when loading the prayer fails', () => {
      const error = new Error('Failed to load prayer');

      const consoleSpy = vi
        .spyOn(console, 'error')
        .mockImplementation(() => {});

      prayersService.getPrayerWithMarkdown.mockReturnValue(
        throwError(() => error),
      );

      fixture.detectChanges();

      expect(consoleSpy).toHaveBeenCalledWith('Error loading prayer: ', error);
    });
  });

  describe('markdown rendering', () => {
    it('should add level two headings to the headings list', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toEqual([
        {
          id: 'history',
          text: 'History',
        },
      ]);
    });

    it('should generate lowercase heading IDs', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '## HISTORY OF PRAYER',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('history-of-prayer');
    });

    it('should replace non-word characters with hyphens in heading IDs', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '## Prayer & Devotion!',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('prayer-devotion-');
    });

    it('should not add level one headings to the headings list', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toHaveLength(1);

      expect(
        component.headings.some((heading) => heading.text === 'Our Father'),
      ).toBe(false);
    });

    it('should not add level three headings to the headings list', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '### Additional information',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toEqual([]);
    });

    it('should render level two headings with the expected ID and classes', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h2 id="history"');

      expect(component.markdownContent).toContain(
        'scroll-mt-32 text-xl font-semibold mt-8 mb-2',
      );
    });

    it('should render other heading levels without custom attributes', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '# Title\n\n### Details',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h1>');
      expect(component.markdownContent).toContain('<h3>');
    });

    it('should generate separate entries for multiple level two headings', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '## History\n\n## Meaning\n\n## Importance',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toEqual([
        { id: 'history', text: 'History' },
        { id: 'meaning', text: 'Meaning' },
        { id: 'importance', text: 'Importance' },
      ]);
    });

    it('should render markdown paragraphs and lists', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: 'A prayer paragraph.\n\n- First item\n- Second item',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<p>');
      expect(component.markdownContent).toContain('<ul>');
      expect(component.markdownContent).toContain('<li>First item</li>');
      expect(component.markdownContent).toContain('<li>Second item</li>');
    });

    it('should handle empty markdown content', async () => {
      prayersService.getPrayerWithMarkdown.mockReturnValue(
        of({
          prayer: mockPrayer,
          markdown: '',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toBe('');
      expect(component.headings).toEqual([]);
    });
  });

  describe('scrollTo', () => {
    it('should not scroll when the target element does not exist', () => {
      const scrollSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('non-existent');

      expect(scrollSpy).not.toHaveBeenCalled();
    });

    it('should scroll the window when there is no scrollable parent', () => {
      const element = document.createElement('div');
      element.id = 'target';

      document.body.appendChild(element);

      Object.defineProperty(element, 'offsetTop', {
        configurable: true,
        value: 500,
      });

      const scrollSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('target');

      expect(scrollSpy).toHaveBeenCalledWith({
        top: 380,
        behavior: 'smooth',
      });
    });

    it('should subtract the header height from the element position', () => {
      const element = document.createElement('div');
      element.id = 'target';

      document.body.appendChild(element);

      Object.defineProperty(element, 'offsetTop', {
        configurable: true,
        value: 320,
      });

      const scrollSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('target');

      expect(scrollSpy).toHaveBeenCalledWith({
        top: 200,
        behavior: 'smooth',
      });
    });

    it('should scroll the nearest parent with overflow-y auto', () => {
      const container = document.createElement('div');
      const element = document.createElement('div');

      container.style.overflowY = 'auto';
      element.id = 'target';

      container.appendChild(element);
      document.body.appendChild(container);

      Object.defineProperty(element, 'offsetTop', {
        configurable: true,
        value: 500,
      });

      const containerSpy = mockElementScrollTo(container);
      const windowSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('target');

      expect(containerSpy).toHaveBeenCalledWith({
        top: 380,
        behavior: 'smooth',
      });

      expect(windowSpy).not.toHaveBeenCalled();
    });

    it('should scroll the nearest parent with overflow-y scroll', () => {
      const container = document.createElement('div');
      const element = document.createElement('div');

      container.style.overflowY = 'scroll';
      element.id = 'target';

      container.appendChild(element);
      document.body.appendChild(container);

      Object.defineProperty(element, 'offsetTop', {
        configurable: true,
        value: 250,
      });

      const containerSpy = mockElementScrollTo(container);

      component.scrollTo('target');

      expect(containerSpy).toHaveBeenCalledWith({
        top: 130,
        behavior: 'smooth',
      });
    });

    it('should prioritize the closest scrollable ancestor', () => {
      const outer = document.createElement('div');
      const inner = document.createElement('div');
      const element = document.createElement('div');

      outer.style.overflowY = 'auto';
      inner.style.overflowY = 'auto';
      element.id = 'target';

      inner.appendChild(element);
      outer.appendChild(inner);
      document.body.appendChild(outer);

      Object.defineProperty(element, 'offsetTop', {
        configurable: true,
        value: 400,
      });

      const innerSpy = mockElementScrollTo(inner);
      const outerSpy = mockElementScrollTo(outer);

      component.scrollTo('target');

      expect(innerSpy).toHaveBeenCalledOnce();
      expect(outerSpy).not.toHaveBeenCalled();
    });
  });
});
