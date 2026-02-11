import {
  Component,
  OnInit,
  AfterViewInit,
  ChangeDetectorRef,
  ViewChild,
  ElementRef,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';
import { NgxMaskDirective } from 'ngx-mask';
import { MarkdownComponent, provideMarkdown } from 'ngx-markdown';

import { SaintsService } from '../../../../core/services/saints.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { TagsService } from '../../../../core/services/tags.service';
import { ReligiousOrdersService } from '../../../../core/services/religious-orders.service';

import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { notOnlyNumbersValidator } from '../../../../shared/validators/notOnlyNumbersValidator';
import { CropDialogComponent } from '../../../../shared/components/crop-dialog/crop-dialog.component';

import { environment } from '../../../../../environments/environment';
import { ReligiousOrder } from '../../../../interfaces/religious-order';
import { Tag } from '../../../../interfaces/tag';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { NewSaintDto } from '../../interfaces/new-saint-dto';
import { CENTURIES } from '../../../../shared/constants/centuries';
import { minMaxLengthValidator } from '../../../../shared/validators/min-max-length.validator';
import { finalize } from 'rxjs';
import { MatInputModule } from '@angular/material/input';
import { personNameValidator } from '../../../../shared/validators/person-name.validator';
import { feastDayValidator } from '../../../../shared/validators/feast-day.validator';

@Component({
  selector: 'app-saint-form-page',
  templateUrl: './saint-form-page.component.html',
  styleUrls: ['./saint-form-page.component.scss'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatMenuModule,
    NgxMaskDirective,
    MarkdownComponent,
    RomanPipe,
    CountryCodePipe,
    ReactiveFormsModule,
    MatInputModule,
  ],
  providers: [provideMarkdown()],
})
export class SaintFormPageComponent implements OnInit, AfterViewInit {
  private saintsService = inject(SaintsService);
  private tagsService = inject(TagsService);
  private religiousOrdersService = inject(ReligiousOrdersService);
  private snackBarService = inject(SnackbarService);
  private dialog = inject(MatDialog);

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  @ViewChild('descriptionTextarea')
  descriptionTextarea!: ElementRef<HTMLTextAreaElement>;

  imageBaseUrl = environment.assetsUrl;
  religiousOrders: ReligiousOrder[] = [];
  tagsList: Tag[] = [];
  currentTags: string[] = [];

  croppedImage: string | null = null;
  isEditMode = false;
  saintId: string | null = null;
  imageLoading = false;
  isSubmitting = false;

  form = this.fb.group({
    name: [
      '',
      [Validators.required, notOnlyNumbersValidator(), personNameValidator],
    ],
    country: ['', [Validators.required, minMaxLengthValidator(3, 150)]],
    century: this.fb.control<number | null>(null, [
      Validators.required,
      minMaxLengthValidator(-20, 21),
    ]),
    image: ['', Validators.required],
    description: ['', [Validators.required, minMaxLengthValidator(1, 200)]],
    markdownContent: [
      '',
      [Validators.required, minMaxLengthValidator(1, 20000)],
    ],
    title: ['', [minMaxLengthValidator(1, 100)]],
    feastDay: ['', [feastDayValidator()]],
    patronOf: ['', minMaxLengthValidator(1, 100)],
    religiousOrder: [''],
  });
  readonly centuries = CENTURIES;

  ngOnInit(): void {
    const filter = new EntityFilters({ type: TagType.Saint, pageSize: 100 });

    this.tagsService.getTags(filter).subscribe((res) => {
      this.tagsList = res.items;
      this.cdr.detectChanges();
    });

    this.religiousOrdersService.getOrders(filter).subscribe((res) => {
      this.religiousOrders = res.items;
      this.cdr.detectChanges();
    });

    this.route.paramMap.subscribe((params) => {
      this.saintId = params.get('id');
      this.isEditMode = !!this.saintId;

      if (this.isEditMode && this.saintId) {
        this.saintsService.getSaintWithMarkdown(this.saintId).subscribe({
          next: ({ saint, markdown }) => {
            this.currentTags = saint.tags.map((tag) => tag.name);

            this.form.patchValue({
              name: saint.name,
              country: saint.country,
              century: saint.century ?? 0,
              image: saint.image,
              description: saint.description,
              markdownContent: markdown,
              title: saint.title ?? '',
              patronOf: saint.patronOf ?? '',
              feastDay: this.saintsService.formatFeastDayFromIso(
                saint.feastDay ?? '',
              ),
              religiousOrder: saint.religiousOrder?.id ?? '',
            });

            setTimeout(() => this.autoResizeOnLoad());
            this.cdr.detectChanges();
          },
          error: () => {
            this.snackBarService.error('Error loading saint for update');
            this.router.navigate(['admin/saints']);
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
    if (this.form.invalid || this.isSubmitting || this.imageLoading) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const dto = this.buildSaintDto();

    const request =
      this.isEditMode && this.saintId
        ? this.saintsService.updateSaint(this.saintId, dto)
        : this.saintsService.createSaint(dto);

    request.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: () => {
        this.snackBarService.success(
          this.isEditMode
            ? 'Saint successfully updated'
            : 'Saint successfully created',
        );
        this.router.navigate(['admin/saints']);
      },
      error: (err) => {
        const errorMessage =
          typeof err.error === 'string'
            ? err.error
            : (err.error?.message ?? 'Unexpected error.');
        this.snackBarService.error(
          `Error ${this.isEditMode ? 'updating' : 'creating'} saint: ${errorMessage}`,
        );
      },
    });
  }

  private buildSaintDto(): NewSaintDto {
    const value = this.form.value;

    return {
      name: value.name!,
      country: value.country!,
      century: Number(value.century),
      image: value.image!,
      description: value.description!,
      markdownContent: value.markdownContent!,
      title: value.title || null,
      patronOf: value.patronOf || null,
      feastDay: value.feastDay
        ? this.saintsService.formatFeastDayToIso(value.feastDay!)
        : undefined,
      religiousOrderId: value.religiousOrder
        ? Number(value.religiousOrder)
        : null,
      tagIds: this.getSelectedTagIds(),
    };
  }

  private getSelectedTagIds(): number[] {
    return this.currentTags
      .map((tagName) =>
        this.tagsList.find(
          (t) => t.name.toLowerCase() === tagName.trim().toLowerCase(),
        ),
      )
      .filter((t): t is Tag => !!t)
      .map((t) => t.id);
  }

  onFileSelected(event: Event, input: HTMLInputElement): void {
    const dialogRef = this.dialog.open(CropDialogComponent, {
      height: '600px',
      width: '600px',
      data: { imageChangedEvent: event },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (typeof result === 'string') {
        this.croppedImage = result;
        this.form.patchValue({ image: result });
        this.form.get('image')?.updateValueAndValidity();
      }
      input.value = '';
    });
  }

  autoResizeOnLoad(): void {
    const textarea = this.descriptionTextarea?.nativeElement;
    if (!textarea) return;
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

  addTag(tag: string): void {
    const trimmed = tag.trim();
    if (
      trimmed &&
      this.currentTags.length < 5 &&
      !this.currentTags.includes(trimmed)
    ) {
      this.currentTags.push(trimmed);
    }
  }

  removeTag(tag: string): void {
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
