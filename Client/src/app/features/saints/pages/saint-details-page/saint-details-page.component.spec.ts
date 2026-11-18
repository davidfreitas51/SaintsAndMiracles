import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

import { SaintDetailsPageComponent } from './saint-details-page.component';
import { SaintsService } from '../../../../core/services/saints.service';
import { environment } from '../../../../../environments/environment';

describe('SaintDetailsPageComponent', () => {
  let component: SaintDetailsPageComponent;
  let fixture: ComponentFixture<SaintDetailsPageComponent>;

  let saintsService: {
    getSaintWithMarkdown: ReturnType<typeof vi.fn>;
  };

  let activatedRoute: {
    snapshot: {
      url: { path: string }[];
    };
  };

  const mockSaint = {
    id: 1,
    name: 'Saint Augustine',
    slug: 'saint-augustine',
    country: 'IT',
  };

  const mockResponse = {
    saint: mockSaint,
    markdown: [
      '# Saint Augustine',
      '',
      'A great doctor of the Church.',
      '',
      '## History',
      '',
      'Saint Augustine was born in 354.',
    ].join('\n'),
  };

  const createRoute = (paths: string[] = ['saints', 'saint-augustine']) => ({
    snapshot: {
      url: paths.map((path) => ({ path })),
    },
  });

  beforeEach(async () => {
    // JSDOM does not implement all browser APIs used by the component.
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      configurable: true,
      value: vi.fn().mockImplementation((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    });

    Object.defineProperty(window, 'scrollTo', {
      writable: true,
      configurable: true,
      value: vi.fn(),
    });

    Object.defineProperty(HTMLElement.prototype, 'scrollTo', {
      writable: true,
      configurable: true,
      value: vi.fn(),
    });

    activatedRoute = createRoute();

    saintsService = {
      getSaintWithMarkdown: vi.fn().mockReturnValue(of(mockResponse)),
    };

    await TestBed.configureTestingModule({
      imports: [SaintDetailsPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        {
          provide: SaintsService,
          useValue: saintsService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SaintDetailsPageComponent);
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
      expect(component.saint).toBeNull();
      expect(component.markdownContent).toBe('');
      expect(component.headings).toEqual([]);
    });

    it('should initialize imageBaseUrl using the environment configuration', () => {
      expect(component.imageBaseUrl).toBe(environment.assetsUrl);
    });
  });

  describe('ngOnInit', () => {
    it('should request the saint using the route slug', () => {
      fixture.detectChanges();

      expect(saintsService.getSaintWithMarkdown).toHaveBeenCalledOnce();
      expect(saintsService.getSaintWithMarkdown).toHaveBeenCalledWith(
        'saint-augustine',
      );
    });

    it('should extract the last segment from a nested route', async () => {
      activatedRoute.snapshot.url = [
        { path: 'saints' },
        { path: 'categories' },
        { path: 'saint-francis' },
      ];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(saintsService.getSaintWithMarkdown).toHaveBeenCalledWith(
        'saint-francis',
      );
    });

    it('should use an empty slug when the route has no segments', async () => {
      activatedRoute.snapshot.url = [];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(saintsService.getSaintWithMarkdown).toHaveBeenCalledWith('');
    });

    it('should assign the returned saint', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.saint).toEqual(mockSaint);
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
      expect(component.markdownContent).toContain('Saint Augustine');
      expect(component.markdownContent).toContain(
        'A great doctor of the Church.',
      );
    });

    it('should log an error when loading the saint fails', () => {
      const error = new Error('Failed to load saint');

      const consoleSpy = vi
        .spyOn(console, 'error')
        .mockImplementation(() => {});

      saintsService.getSaintWithMarkdown.mockReturnValue(
        throwError(() => error),
      );

      fixture.detectChanges();

      expect(consoleSpy).toHaveBeenCalledWith('Error loading saint: ', error);
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
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '## LIFE OF THE SAINT',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('life-of-the-saint');
    });

    it('should replace non-word characters with hyphens in heading IDs', async () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '## Faith & Devotion!',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('faith-devotion-');
    });

    it('should not add level one headings to the headings list', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(
        component.headings.some(
          (heading) => heading.text === 'Saint Augustine',
        ),
      ).toBe(false);
    });

    it('should not add level three headings to the headings list', async () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '### Early Life',
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
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '# Title\n\n### Details',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h1>');
      expect(component.markdownContent).toContain('<h3>');
    });

    it('should generate separate entries for multiple level two headings', async () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '## History\n\n## Miracles\n\n## Legacy',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toEqual([
        { id: 'history', text: 'History' },
        { id: 'miracles', text: 'Miracles' },
        { id: 'legacy', text: 'Legacy' },
      ]);
    });

    it('should render markdown paragraphs and lists', async () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
          markdown: 'Saint biography.\n\n- First event\n- Second event',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<p>');
      expect(component.markdownContent).toContain('<ul>');
      expect(component.markdownContent).toContain('<li>First event</li>');
      expect(component.markdownContent).toContain('<li>Second event</li>');
    });

    it('should handle empty markdown content', async () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: mockSaint,
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

      const containerSpy = vi.spyOn(container, 'scrollTo');
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

      const containerSpy = vi.spyOn(container, 'scrollTo');

      component.scrollTo('target');

      expect(containerSpy).toHaveBeenCalledWith({
        top: 130,
        behavior: 'smooth',
      });
    });
  });
});
