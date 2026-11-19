import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  ParamMap,
  Router,
} from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { provideNgxMask } from 'ngx-mask';

vi.mock('ngx-image-cropper', async () => {
  const { Component } = await import('@angular/core');

  @Component({
    selector: 'image-cropper',
    standalone: true,
    template: '',
  })
  class MockImageCropperComponent {}

  return {
    ImageCropperComponent: MockImageCropperComponent,
  };
});

import { SaintFormPageComponent } from './saint-form-page.component';
import { SaintsService } from '../../../../core/services/saints.service';
import { TagsService } from '../../../../core/services/tags.service';
import { ReligiousOrdersService } from '../../../../core/services/religious-orders.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';

describe('SaintFormPageComponent', () => {
  let component: SaintFormPageComponent;
  let fixture: ComponentFixture<SaintFormPageComponent>;

  let saintsService: {
    getSaintWithMarkdown: ReturnType<typeof vi.fn>;
    createSaint: ReturnType<typeof vi.fn>;
    updateSaint: ReturnType<typeof vi.fn>;
    formatFeastDayFromIso: ReturnType<typeof vi.fn>;
    formatFeastDayToIso: ReturnType<typeof vi.fn>;
  };

  let tagsService: {
    getTags: ReturnType<typeof vi.fn>;
  };

  let religiousOrdersService: {
    getOrders: ReturnType<typeof vi.fn>;
  };

  let snackBarService: {
    success: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
  };

  let router: {
    navigate: ReturnType<typeof vi.fn>;
  };

  let dialog: {
    open: ReturnType<typeof vi.fn>;
  };

  let routeParams: BehaviorSubject<ParamMap>;

  const mockSaint = {
    id: 1,
    name: 'Saint Augustine',
    country: 'Algeria',
    century: 4,
    image: 'augustine.png',
    description: 'Bishop and Doctor of the Church',
    title: 'Doctor of Grace',
    patronOf: 'Theologians',
    feastDay: '2026-08-28',
    religiousOrder: { id: 2, name: 'Augustinians' },
    tags: [{ id: 1, name: 'Doctor of the Church' }],
  };

  const mockTags = {
    items: [
      { id: 1, name: 'Doctor of the Church' },
      { id: 2, name: 'Bishop' },
      { id: 3, name: 'Theologian' },
    ],
  };

  const mockOrders = {
    items: [
      { id: 2, name: 'Augustinians' },
      { id: 3, name: 'Benedictines' },
    ],
  };

  const validFormValue = {
    name: 'Saint Augustine',
    country: 'Algeria',
    century: 4,
    image: 'data:image/png;base64,image-data',
    description: 'Bishop and Doctor of the Church',
    markdownContent: '# Saint Augustine\n\nSome detailed content.',
    title: 'Doctor of Grace',
    feastDay: '2808',
    patronOf: 'Theologians',
    religiousOrder: '2',
  };

  beforeEach(async () => {
    routeParams = new BehaviorSubject<ParamMap>(convertToParamMap({}));

    saintsService = {
      getSaintWithMarkdown: vi.fn().mockReturnValue(
        of({
          saint: mockSaint,
          markdown: '# Saint Augustine\n\nBiography content.',
        }),
      ),
      createSaint: vi.fn().mockReturnValue(of({})),
      updateSaint: vi.fn().mockReturnValue(of({})),
      formatFeastDayFromIso: vi.fn().mockReturnValue('2808'),
      formatFeastDayToIso: vi.fn().mockReturnValue('2026-08-28'),
    };

    tagsService = {
      getTags: vi.fn().mockReturnValue(of(mockTags)),
    };

    religiousOrdersService = {
      getOrders: vi.fn().mockReturnValue(of(mockOrders)),
    };

    snackBarService = {
      success: vi.fn(),
      error: vi.fn(),
    };

    router = {
      navigate: vi.fn().mockResolvedValue(true),
    };

    dialog = {
      open: vi.fn().mockReturnValue({
        afterClosed: () => of(null),
      }),
    };

    await TestBed.configureTestingModule({
      imports: [SaintFormPageComponent],
      providers: [
        provideNgxMask(),
        {
          provide: ActivatedRoute,
          useValue: { paramMap: routeParams.asObservable() },
        },
        { provide: Router, useValue: router },
        { provide: SaintsService, useValue: saintsService },
        { provide: TagsService, useValue: tagsService },
        {
          provide: ReligiousOrdersService,
          useValue: religiousOrdersService,
        },
        { provide: SnackbarService, useValue: snackBarService },
        { provide: MatDialog, useValue: dialog },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SaintFormPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture?.destroy();
    vi.restoreAllMocks();
  });

  describe('initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should load tags and religious orders', () => {
      expect(tagsService.getTags).toHaveBeenCalledOnce();
      expect(religiousOrdersService.getOrders).toHaveBeenCalledOnce();
      expect(component.tagsList).toEqual(mockTags.items);
      expect(component.religiousOrders).toEqual(mockOrders.items);
    });

    it('should initialize in create mode when there is no route id', () => {
      expect(component.isEditMode).toBe(false);
      expect(component.saintId).toBeNull();
      expect(saintsService.getSaintWithMarkdown).not.toHaveBeenCalled();
    });

    it('should load the saint when an id is present in the route', () => {
      routeParams.next(convertToParamMap({ id: 'augustine' }));
      fixture.detectChanges();

      expect(component.isEditMode).toBe(true);
      expect(component.saintId).toBe('augustine');
      expect(saintsService.getSaintWithMarkdown).toHaveBeenCalledWith(
        'augustine',
      );
    });

    it('should populate the form when editing a saint', () => {
      routeParams.next(convertToParamMap({ id: 'augustine' }));
      fixture.detectChanges();

      expect(component.form.getRawValue()).toMatchObject({
        name: mockSaint.name,
        country: mockSaint.country,
        century: mockSaint.century,
        image: mockSaint.image,
        description: mockSaint.description,
        markdownContent: '# Saint Augustine\n\nBiography content.',
        title: mockSaint.title,
        patronOf: mockSaint.patronOf,
        feastDay: '2808',
        religiousOrder: 2,
      });

      expect(component.currentTags).toEqual(['Doctor of the Church']);
      expect(saintsService.formatFeastDayFromIso).toHaveBeenCalledWith(
        mockSaint.feastDay,
      );
    });

    it('should use defaults for optional saint properties', () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        of({
          saint: {
            ...mockSaint,
            title: null,
            patronOf: null,
            feastDay: null,
            religiousOrder: null,
            century: null,
            tags: [],
          },
          markdown: '# Biography',
        }),
      );

      routeParams.next(convertToParamMap({ id: 'augustine' }));
      fixture.detectChanges();

      expect(component.form.controls.title.value).toBe('');
      expect(component.form.controls.patronOf.value).toBe('');
      expect(component.form.controls.feastDay.value).toBe('2808');
      expect(component.form.controls.religiousOrder.value).toBe('');
      expect(component.form.controls.century.value).toBe(0);
      expect(component.currentTags).toEqual([]);
    });

    it('should show an error and navigate away when saint loading fails', () => {
      saintsService.getSaintWithMarkdown.mockReturnValue(
        throwError(() => new Error('Request failed')),
      );

      routeParams.next(convertToParamMap({ id: 'invalid-id' }));
      fixture.detectChanges();

      expect(snackBarService.error).toHaveBeenCalledWith(
        'Error loading saint for update',
      );
      expect(router.navigate).toHaveBeenCalledWith(['admin/saints']);
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.form.setValue(validFormValue);
    });

    it('should create a saint when the form is valid and in create mode', () => {
      component.onSubmit();

      expect(saintsService.createSaint).toHaveBeenCalledOnce();
      expect(saintsService.updateSaint).not.toHaveBeenCalled();
      expect(snackBarService.success).toHaveBeenCalledWith(
        'Saint successfully created',
      );
      expect(router.navigate).toHaveBeenCalledWith(['admin/saints']);
      expect(component.isSubmitting).toBe(false);
    });

    it('should update a saint when in edit mode', () => {
      component.isEditMode = true;
      component.saintId = 'augustine';

      component.onSubmit();

      expect(saintsService.updateSaint).toHaveBeenCalledOnce();
      expect(saintsService.updateSaint).toHaveBeenCalledWith(
        'augustine',
        expect.any(Object),
      );
      expect(saintsService.createSaint).not.toHaveBeenCalled();
      expect(snackBarService.success).toHaveBeenCalledWith(
        'Saint successfully updated',
      );
    });

    it('should not submit when the form is invalid', () => {
      component.form.controls.name.setValue('');

      component.onSubmit();

      expect(saintsService.createSaint).not.toHaveBeenCalled();
      expect(component.form.controls.name.touched).toBe(true);
    });

    it('should not submit when already submitting', () => {
      component.isSubmitting = true;

      component.onSubmit();

      expect(saintsService.createSaint).not.toHaveBeenCalled();
    });

    it('should not submit while an image is loading', () => {
      component.imageLoading = true;

      component.onSubmit();

      expect(saintsService.createSaint).not.toHaveBeenCalled();
    });

    it('should display the server error when the request fails with a string', () => {
      saintsService.createSaint.mockReturnValue(
        throwError(() => ({ error: 'Name already exists' })),
      );

      component.onSubmit();

      expect(snackBarService.error).toHaveBeenCalledWith(
        'Error creating saint: Name already exists',
      );
      expect(component.isSubmitting).toBe(false);
    });

    it('should display the server message when the request fails with an object', () => {
      saintsService.createSaint.mockReturnValue(
        throwError(() => ({
          error: { message: 'Invalid saint data' },
        })),
      );

      component.onSubmit();

      expect(snackBarService.error).toHaveBeenCalledWith(
        'Error creating saint: Invalid saint data',
      );
    });

    it('should display a fallback message when the request error has no message', () => {
      saintsService.createSaint.mockReturnValue(
        throwError(() => ({ error: {} })),
      );

      component.onSubmit();

      expect(snackBarService.error).toHaveBeenCalledWith(
        'Error creating saint: Unexpected error.',
      );
    });

    it('should use the update wording when an update request fails', () => {
      component.isEditMode = true;
      component.saintId = 'augustine';

      saintsService.updateSaint.mockReturnValue(
        throwError(() => ({ error: 'Update failed' })),
      );

      component.onSubmit();

      expect(snackBarService.error).toHaveBeenCalledWith(
        'Error updating saint: Update failed',
      );
      expect(component.isSubmitting).toBe(false);
    });
  });

  describe('buildSaintDto through onSubmit', () => {
    it('should build the DTO with the expected values', () => {
      component.form.setValue(validFormValue);
      component.currentTags = ['Doctor of the Church', 'Bishop'];

      component.onSubmit();

      expect(saintsService.createSaint).toHaveBeenCalledWith({
        name: 'Saint Augustine',
        country: 'Algeria',
        century: 4,
        image: 'data:image/png;base64,image-data',
        description: 'Bishop and Doctor of the Church',
        markdownContent: '# Saint Augustine\n\nSome detailed content.',
        title: 'Doctor of Grace',
        patronOf: 'Theologians',
        feastDay: '2026-08-28',
        religiousOrderId: 2,
        tagIds: [1, 2],
      });

      expect(saintsService.formatFeastDayToIso).toHaveBeenCalledWith('2808');
    });

    it('should send null for empty optional text fields', () => {
      component.form.setValue({
        ...validFormValue,
        title: '',
        patronOf: '',
        feastDay: '',
        religiousOrder: '',
      });

      component.onSubmit();

      expect(saintsService.createSaint).toHaveBeenCalledWith(
        expect.objectContaining({
          title: null,
          patronOf: null,
          feastDay: undefined,
          religiousOrderId: null,
        }),
      );

      expect(saintsService.formatFeastDayToIso).not.toHaveBeenCalled();
    });

    it('should convert the religious order id to a string', () => {
      component.form.setValue({
        ...validFormValue,
        religiousOrder: '3',
      });

      component.onSubmit();

      expect(saintsService.createSaint).toHaveBeenCalledWith(
        expect.objectContaining({ religiousOrderId: 3 }),
      );
    });

    it('should include only tag ids that exist in the loaded tag list', () => {
      component.form.setValue(validFormValue);
      component.currentTags = ['Doctor of the Church', 'Unknown tag', 'Bishop'];

      component.onSubmit();

      expect(saintsService.createSaint).toHaveBeenCalledWith(
        expect.objectContaining({ tagIds: [1, 2] }),
      );
    });
  });

  describe('tags', () => {
    it('should add a trimmed tag', () => {
      component.addTag('  Bishop  ');

      expect(component.currentTags).toContain('Bishop');
    });

    it('should not add an empty tag', () => {
      component.addTag('   ');

      expect(component.currentTags).toEqual([]);
    });

    it('should not add a duplicate tag', () => {
      component.currentTags = ['Bishop'];

      component.addTag('Bishop');

      expect(component.currentTags).toEqual(['Bishop']);
    });

    it('should not add a tag that already exists with surrounding whitespace', () => {
      component.currentTags = ['Bishop'];

      component.addTag('  Bishop  ');

      expect(component.currentTags).toEqual(['Bishop']);
    });

    it('should not add more than five tags', () => {
      component.currentTags = ['A', 'B', 'C', 'D', 'E'];

      component.addTag('F');

      expect(component.currentTags).toHaveLength(5);
      expect(component.currentTags).not.toContain('F');
    });

    it('should remove a tag', () => {
      component.currentTags = ['Bishop', 'Theologian'];

      component.removeTag('Bishop');

      expect(component.currentTags).toEqual(['Theologian']);
    });

    it('should do nothing when removing a tag that does not exist', () => {
      component.currentTags = ['Bishop'];

      component.removeTag('Unknown');

      expect(component.currentTags).toEqual(['Bishop']);
    });
  });

  describe('image handling', () => {
    it('should open the crop dialog when a file is selected', async () => {
      const event = new Event('change');
      const input = document.createElement('input');

      await component.onFileSelected(event, input);

      expect(dialog.open).toHaveBeenCalledOnce();
      expect(dialog.open).toHaveBeenCalledWith(
        expect.any(Function),
        expect.objectContaining({
          height: '600px',
          width: '600px',
          data: { imageChangedEvent: event },
        }),
      );
    });

    it('should set the image when the crop dialog returns a data URL', async () => {
      dialog.open.mockReturnValue({
        afterClosed: () => of('data:image/png;base64,cropped-image'),
      });

      const input = document.createElement('input');
      input.value = 'selected-file.png';

      await component.onFileSelected(new Event('change'), input);

      expect(component.form.controls.image.value).toBe(
        'data:image/png;base64,cropped-image',
      );
      expect(input.value).toBe('');
    });

    it('should not change the image when the crop dialog is cancelled', async () => {
      component.form.controls.image.setValue('existing-image.png');

      dialog.open.mockReturnValue({
        afterClosed: () => of(null),
      });

      await component.onFileSelected(
        new Event('change'),
        document.createElement('input'),
      );

      expect(component.form.controls.image.value).toBe('existing-image.png');
    });

    it('should not change the image when the dialog returns an invalid result', async () => {
      component.form.controls.image.setValue('existing-image.png');

      dialog.open.mockReturnValue({
        afterClosed: () => of('not-an-image'),
      });

      await component.onFileSelected(
        new Event('change'),
        document.createElement('input'),
      );

      expect(component.form.controls.image.value).toBe('existing-image.png');
    });

    it('should return an empty preview when there is no image', () => {
      component.form.controls.image.setValue('');

      expect(component.getImagePreview()).toBe('');
    });

    it('should return a data URL directly as the image preview', () => {
      component.form.controls.image.setValue(
        'data:image/png;base64,image-data',
      );

      expect(component.getImagePreview()).toBe(
        'data:image/png;base64,image-data',
      );
    });

    it('should return an HTTP URL directly as the image preview', () => {
      component.form.controls.image.setValue('https://example.com/image.png');

      expect(component.getImagePreview()).toBe('https://example.com/image.png');
    });

    it('should prepend the assets URL to a relative image path', () => {
      component.form.controls.image.setValue('saints/augustine.png');

      expect(component.getImagePreview()).toBe(
        component.imageBaseUrl + 'saints/augustine.png',
      );
    });
  });

  describe('textarea resizing', () => {
    it('should resize the description textarea on load', () => {
      const textarea = document.createElement('textarea');

      Object.defineProperty(textarea, 'scrollHeight', {
        configurable: true,
        value: 120,
      });

      component.descriptionTextarea = {
        nativeElement: textarea,
      };

      component.autoResizeOnLoad();

      expect(textarea.style.height).toBe('120px');
    });

    it('should do nothing when the description textarea is unavailable', () => {
      component.descriptionTextarea = undefined!;

      expect(() => component.autoResizeOnLoad()).not.toThrow();
    });

    it('should resize a textarea from an input event', () => {
      const textarea = document.createElement('textarea');

      Object.defineProperty(textarea, 'scrollHeight', {
        configurable: true,
        value: 150,
      });

      component.autoResize({ target: textarea } as unknown as Event);

      expect(textarea.style.height).toBe('150px');
    });
  });

  describe('markdown', () => {
    it('should expose the markdown form control', () => {
      expect(component.markdownContent).toBe(
        component.form.get('markdownContent'),
      );
    });

    it('should insert markdown around the selected text', () => {
      const textarea = document.createElement('textarea');
      textarea.value = 'Hello world';
      textarea.setSelectionRange(6, 11);

      component.form.controls.markdownContent.setValue('Hello world');

      const querySelectorSpy = vi
        .spyOn(document, 'querySelector')
        .mockReturnValue(textarea);

      component.insertMarkdown('**', '**');

      expect(component.markdownContent.value).toBe('Hello **world**');
      expect(querySelectorSpy).toHaveBeenCalled();

      querySelectorSpy.mockRestore();
    });

    it('should insert markdown at the cursor when no text is selected', () => {
      const textarea = document.createElement('textarea');
      textarea.value = 'Hello world';
      textarea.setSelectionRange(5, 5);

      component.form.controls.markdownContent.setValue('Hello world');

      const querySelectorSpy = vi
        .spyOn(document, 'querySelector')
        .mockReturnValue(textarea);

      component.insertMarkdown('**', '**');

      expect(component.markdownContent.value).toBe('Hello**** world');
      expect(querySelectorSpy).toHaveBeenCalled();

      querySelectorSpy.mockRestore();
    });

    it('should do nothing when the markdown textarea is not in the document', () => {
      component.form.controls.markdownContent.setValue('Original content');

      vi.spyOn(document, 'querySelector').mockReturnValue(null);

      component.insertMarkdown('**', '**');

      expect(component.markdownContent.value).toBe('Original content');
    });

    it('should do nothing when the markdown control is unavailable', () => {
      const getSpy = vi.spyOn(component.form, 'get').mockReturnValue(null);

      expect(() => component.insertMarkdown('**', '**')).not.toThrow();

      getSpy.mockRestore();
    });
  });
});
