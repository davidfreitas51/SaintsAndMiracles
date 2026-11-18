import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

import { MiracleDetailsPageComponent } from './miracle-details-page.component';
import { MiraclesService } from '../../../../core/services/miracles.service';

describe('MiracleDetailsPageComponent', () => {
  let component: MiracleDetailsPageComponent;
  let fixture: ComponentFixture<MiracleDetailsPageComponent>;

  let miraclesService: {
    getMiracleWithMarkdown: ReturnType<typeof vi.fn>;
  };

  let activatedRoute: {
    snapshot: {
      url: { path: string }[];
    };
  };

  const mockMiracle = {
    id: 1,
    name: 'Miracle of Example',
    country: 'US',
  };

  const mockResponse = {
    miracle: mockMiracle,
    markdown: '# Miracle\n\n## History\n\nSome content.',
  };

  const createRoute = (
    paths: string[] = ['miracles', 'miracle-of-example'],
  ) => ({
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

    miraclesService = {
      getMiracleWithMarkdown: vi.fn().mockReturnValue(of(mockResponse)),
    };

    await TestBed.configureTestingModule({
      imports: [MiracleDetailsPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        {
          provide: MiraclesService,
          useValue: miraclesService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MiracleDetailsPageComponent);
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
      expect(component.miracle).toBeNull();
      expect(component.markdownContent).toBe('');
      expect(component.headings).toEqual([]);
    });

    it('should load the miracle using the route slug', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(miraclesService.getMiracleWithMarkdown).toHaveBeenCalledOnce();

      expect(miraclesService.getMiracleWithMarkdown).toHaveBeenCalledWith(
        'miracle-of-example',
      );
    });

    it('should extract the last segment from a nested route', async () => {
      activatedRoute.snapshot.url = [
        { path: 'admin' },
        { path: 'miracles' },
        { path: 'nested-miracle' },
      ];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(miraclesService.getMiracleWithMarkdown).toHaveBeenCalledWith(
        'nested-miracle',
      );
    });

    it('should use an empty slug when the route has no segments', async () => {
      activatedRoute.snapshot.url = [];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(miraclesService.getMiracleWithMarkdown).toHaveBeenCalledWith('');
    });
  });

  describe('miracle loading', () => {
    it('should assign the returned miracle', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.miracle).toEqual(mockMiracle);
    });

    it('should clear existing headings before processing new content', async () => {
      component.headings = [{ id: 'old-heading', text: 'Old heading' }];

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).not.toContainEqual({
        id: 'old-heading',
        text: 'Old heading',
      });
    });

    it('should process markdown content', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h1');
      expect(component.markdownContent).toContain('Miracle');
      expect(component.markdownContent).toContain('Some content.');
    });

    it('should log an error when miracle loading fails', () => {
      const error = new Error('Failed to load miracle');

      const consoleSpy = vi
        .spyOn(console, 'error')
        .mockImplementation(() => {});

      miraclesService.getMiracleWithMarkdown.mockReturnValue(
        throwError(() => error),
      );

      fixture.detectChanges();

      expect(consoleSpy).toHaveBeenCalledWith('Error loading miracle: ', error);
    });
  });

  describe('markdown rendering', () => {
    it('should generate heading entries for level two headings', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toContainEqual({
        id: 'history',
        text: 'History',
      });
    });

    it('should generate heading IDs using lowercase text', async () => {
      miraclesService.getMiracleWithMarkdown.mockReturnValue(
        of({
          miracle: mockMiracle,
          markdown: '## HISTORY OF THE MIRACLE',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('history-of-the-miracle');
    });

    it('should replace non-word characters with hyphens in heading IDs', async () => {
      miraclesService.getMiracleWithMarkdown.mockReturnValue(
        of({
          miracle: mockMiracle,
          markdown: '## History & Background!',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings[0].id).toBe('history-background-');
    });

    it('should not add level one headings to the headings list', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(
        component.headings.some((heading) => heading.text === 'Miracle'),
      ).toBe(false);
    });

    it('should not add level three headings to the headings list', async () => {
      miraclesService.getMiracleWithMarkdown.mockReturnValue(
        of({
          miracle: mockMiracle,
          markdown: '### Details',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.headings).toEqual([]);
    });

    it('should render level two headings with the expected attributes', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h2 id="history"');

      expect(component.markdownContent).toContain('scroll-mt-32');

      expect(component.markdownContent).toContain(
        'text-xl font-semibold mt-8 mb-2',
      );
    });

    it('should render headings of other levels without custom attributes', async () => {
      miraclesService.getMiracleWithMarkdown.mockReturnValue(
        of({
          miracle: mockMiracle,
          markdown: '# Title\n\n### Details',
        }),
      );

      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.markdownContent).toContain('<h1>');
      expect(component.markdownContent).toContain('<h3>');
    });
  });

  describe('scrollTo', () => {
    it('should do nothing when the target element does not exist', () => {
      const scrollSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('non-existent');

      expect(scrollSpy).not.toHaveBeenCalled();
    });

    it('should scroll the window when no scrollable parent exists', () => {
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

    it('should account for the header height when scrolling', () => {
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

    it('should scroll the nearest scrollable parent instead of the window', () => {
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

      const containerScrollSpy = mockElementScrollTo(container);
      const windowScrollSpy = vi.spyOn(window, 'scrollTo');

      component.scrollTo('target');

      expect(containerScrollSpy).toHaveBeenCalledWith({
        top: 380,
        behavior: 'smooth',
      });

      expect(windowScrollSpy).not.toHaveBeenCalled();
    });

    it('should detect a parent with overflow-y scroll', () => {
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

      const scrollSpy = mockElementScrollTo(container);

      component.scrollTo('target');

      expect(scrollSpy).toHaveBeenCalledWith({
        top: 130,
        behavior: 'smooth',
      });
    });

    it('should use the nearest scrollable ancestor', () => {
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
