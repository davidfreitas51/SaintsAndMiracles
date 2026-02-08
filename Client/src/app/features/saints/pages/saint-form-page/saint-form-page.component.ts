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
  form!: FormGroup;
  isEditMode = false;
  saintId: string | null = null;
  imageLoading = false;

  readonly centuries = CENTURIES;

  ngOnInit(): void {
    this.loadTagsAndOrders();
    this.initForm();
    this.listenDescriptionChanges();
    this.checkEditMode();
  }

  ngAfterViewInit() {
    this.autoResizeOnLoad();
  }

  private loadTagsAndOrders(): void {
    const filter = new EntityFilters({ type: TagType.Saint, pageSize: 100 });

    this.tagsService
      .getTags(filter)
      .subscribe((res) => (this.tagsList = res.items));
    this.religiousOrdersService
      .getOrders(filter)
      .subscribe((res) => (this.religiousOrders = res.items));
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, notOnlyNumbersValidator()]],
      country: ['', Validators.required],
      century: [null, Validators.required],
      image: ['', Validators.required],
      description: ['', Validators.required],
      markdownContent: ['', Validators.required],
      title: [''],
      feastDay: [''], // sempre no formato dd/MM
      patronOf: [''],
      religiousOrder: [''],
    });
  }

  private listenDescriptionChanges(): void {
    this.form
      .get('description')
      ?.valueChanges.subscribe(() =>
        setTimeout(() => this.autoResizeOnLoad(), 0),
      );
  }

  private checkEditMode(): void {
    this.route.paramMap.subscribe((params) => {
      this.saintId = params.get('id');
      this.isEditMode = !!this.saintId;

      if (!this.isEditMode || !this.saintId) {
        this.cdr.detectChanges();
        return;
      }

      this.saintsService.getSaintWithMarkdown(this.saintId).subscribe({
        next: ({ saint, markdown }) => {
          this.currentTags = saint.tags.map((tag) => tag.name);
          this.form.patchValue({
            name: saint.name,
            country: saint.country,
            century: saint.century,
            image: saint.image,
            description: saint.description,
            markdownContent: markdown,
            title: saint.title,
            patronOf: saint.patronOf,
            feastDay: this.saintsService.formatFeastDayFromIso(
              saint.feastDay || '',
            ),
            religiousOrder: saint.religiousOrder?.id,
          });
          this.cdr.detectChanges();
          setTimeout(() => this.autoResizeOnLoad(), 100);
        },
        error: () => {
          this.snackBarService.error('Error loading saint for update');
          this.router.navigate(['admin/saints']);
        },
      });
    });
  }

  onSubmit(): void {
    if (this.imageLoading) return;

    const tagIds = this.currentTags
      .map((tagName) =>
        this.tagsList.find(
          (t) => t.name.toLowerCase() === tagName.trim().toLowerCase(),
        ),
      )
      .filter((t): t is Tag => !!t)
      .map((t) => t.id);

    const saintData: NewSaintDto & { feastDay?: string } = {
      ...this.form.value,
      century: +this.form.value.century,
      title: this.form.value.title || undefined,
      patronOf: this.form.value.patronOf || undefined,
      religiousOrderId: this.form.value.religiousOrder || undefined,
      tagIds,
    };

    const request$ =
      this.isEditMode && this.saintId
        ? this.saintsService.updateSaint(this.saintId, saintData)
        : this.saintsService.createSaint(saintData);

    request$.subscribe({
      next: () => {
        this.snackBarService.success(
          `Saint successfully ${this.isEditMode ? 'updated' : 'created'}`,
        );
        this.router.navigate(['admin/saints']);
      },
      error: (err) => {
        console.error(err);
        const msg =
          typeof err.error === 'string'
            ? err.error
            : (err.error?.message ?? 'Unexpected error.');
        this.snackBarService.error(
          `Error ${this.isEditMode ? 'updating' : 'creating'} saint: ${msg}`,
        );
      },
    });
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
