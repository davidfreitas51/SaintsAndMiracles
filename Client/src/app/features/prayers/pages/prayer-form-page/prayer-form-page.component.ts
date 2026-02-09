import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject,
  ViewChild,
  ElementRef,
  AfterViewInit,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatSelectModule } from '@angular/material/select';
import { PrayersService } from '../../../../core/services/prayers.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { TagsService } from '../../../../core/services/tags.service';
import { environment } from '../../../../../environments/environment';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { CropDialogComponent } from '../../../../shared/components/crop-dialog/crop-dialog.component';
import { Tag } from '../../../../interfaces/tag';
import { MatMenuModule } from '@angular/material/menu';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { MarkdownComponent, provideMarkdown } from 'ngx-markdown';
import { notOnlyNumbersValidator } from '../../../../shared/validators/notOnlyNumbersValidator';
import { MatInputModule } from '@angular/material/input';
import { minMaxLengthValidator } from '../../../../shared/validators/min-max-length.validator';

@Component({
  selector: 'app-prayer-form-page',
  templateUrl: './prayer-form-page.component.html',
  styleUrls: ['./prayer-form-page.component.scss'],
  standalone: true,
  imports: [
    MatMenuModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    RouterModule,
    MarkdownComponent,
    MatInputModule,
  ],
  providers: [provideMarkdown()],
})
export class PrayerFormPageComponent implements OnInit, AfterViewInit {
  private fb = inject(FormBuilder);
  private prayersService = inject(PrayersService);
  private snackBarService = inject(SnackbarService);
  private tagsService = inject(TagsService);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  tagsList: Tag[] = [];
  currentTags: string[] = [];

  @ViewChild('descriptionTextarea')
  descriptionTextarea!: ElementRef<HTMLTextAreaElement>;

  imageBaseUrl = environment.assetsUrl;
  croppedImage: string | null = null;
  isEditMode = false;
  prayerId: string | null = null;
  imageLoading = false;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    title: [
      '',
      [
        Validators.required,
        minMaxLengthValidator(3, 150),
        notOnlyNumbersValidator(),
      ],
    ],
    description: ['', [Validators.required, minMaxLengthValidator(1, 200)]],
    markdownContent: [
      '',
      [Validators.required, minMaxLengthValidator(1, 20000)],
    ],
    image: ['', Validators.required],
  });

  ngOnInit(): void {
    const filter = new EntityFilters({ type: TagType.Prayer });
    filter.pageSize = 100;
    this.tagsService.getTags(filter).subscribe((res) => {
      this.tagsList = res.items;
      this.cdr.detectChanges();
    });

    this.route.paramMap.subscribe((params) => {
      this.prayerId = params.get('id');
      this.isEditMode = !!this.prayerId;

      if (this.isEditMode && this.prayerId) {
        this.prayersService.getPrayerWithMarkdown(this.prayerId).subscribe({
          next: ({ prayer, markdown }) => {
            this.currentTags = prayer.tags.map((tag) => tag.name);
            this.form.patchValue({
              title: prayer.title,
              description: prayer.description,
              markdownContent: markdown,
              image: prayer.image,
            });
            setTimeout(() => this.autoResizeOnLoad());
            this.cdr.detectChanges();
          },
          error: () => {
            this.snackBarService.error('Error loading prayer for update');
            this.router.navigate(['admin/prayers']);
          },
        });
      } else {
        this.cdr.detectChanges();
      }
    });
  }

  ngAfterViewInit() {
    this.autoResizeOnLoad();
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const dto = this.buildPrayerDto();

    const request =
      this.isEditMode && this.prayerId
        ? this.prayersService.updatePrayer(this.prayerId, dto)
        : this.prayersService.createPrayer(dto);

    request.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: () => {
        this.snackBarService.success(
          this.isEditMode
            ? 'Prayer successfully updated'
            : 'Prayer successfully created',
        );
        this.router.navigate(['admin/prayers']);
      },
      error: (err) => {
        const errorMessage =
          typeof err.error === 'string'
            ? err.error
            : (err.error?.message ?? 'Unexpected error.');
        this.snackBarService.error(
          `Error ${this.isEditMode ? 'updating' : 'creating'} prayer: ${errorMessage}`,
        );
      },
    });
  }

  private buildPrayerDto() {
    const tagIds: number[] = this.currentTags
      .map((tagName) => this.tagsList.find((t) => t.name === tagName))
      .filter((t): t is Tag => !!t)
      .map((t) => t.id);

    return {
      title: this.form.controls.title.value.trim(),
      description: this.form.controls.description.value.trim(),
      markdownContent: this.form.controls.markdownContent.value,
      image: this.form.controls.image.value,
      tagIds,
    };
  }

  onFileSelected(event: Event, input: HTMLInputElement): void {
    const dialogRef = this.dialog.open(CropDialogComponent, {
      height: '600px',
      width: '600px',
      data: { imageChangedEvent: event },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && typeof result === 'string') {
        this.croppedImage = result;
        this.form.patchValue({ image: result });
        this.form.get('image')?.updateValueAndValidity();
      } else {
        console.error('Unexpected result format:', result);
      }
      input.value = '';
    });
  }

  autoResizeOnLoad() {
    if (!this.descriptionTextarea) return;
    const textarea = this.descriptionTextarea.nativeElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  autoResize(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  get markdownContent(): FormControl {
    return this.form.get('markdownContent') as FormControl;
  }

  getImagePreview(): string {
    const img = this.form.get('image')?.value;
    if (!img) return '';
    return img.startsWith('data:image') || img.startsWith('http')
      ? img
      : this.imageBaseUrl + img;
  }

  addTag(tag: string) {
    const trimmed = tag.trim();
    if (
      trimmed &&
      this.currentTags.length < 5 &&
      !this.currentTags.includes(trimmed)
    ) {
      this.currentTags.push(trimmed);
    }
  }

  removeTag(tag: string) {
    this.currentTags = this.currentTags.filter((t) => t !== tag);
  }

  insertMarkdown(start: string, end: string = ''): void {
    const control = this.form.get('markdownContent');
    if (!control) return;

    const textarea = document.querySelector<HTMLTextAreaElement>(
      'textarea[formControlName="markdownContent"]',
    );
    if (!textarea) return;

    const { selectionStart, selectionEnd, value } = textarea;
    const selectedText = value.substring(selectionStart, selectionEnd);

    const newText = start + selectedText + end;

    control.setValue(
      value.substring(0, selectionStart) +
        newText +
        value.substring(selectionEnd),
    );

    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(
        selectionStart + start.length,
        selectionEnd + start.length,
      );
    }, 0);
  }
}
