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
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatSelectModule } from '@angular/material/select';
import { MiraclesService } from '../../../../core/services/miracles.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { TagsService } from '../../../../core/services/tags.service';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { environment } from '../../../../../environments/environment';
import { CommonModule } from '@angular/common';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { MatDialog } from '@angular/material/dialog';
import { CropDialogComponent } from '../../../../shared/components/crop-dialog/crop-dialog.component';
import { Tag } from '../../../../interfaces/tag';
import { MatMenuModule } from '@angular/material/menu';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { MarkdownComponent, provideMarkdown } from 'ngx-markdown';
import { notOnlyNumbersValidator } from '../../../../shared/validators/notOnlyNumbersValidator';

@Component({
  selector: 'app-miracle-form-page',
  templateUrl: './miracle-form-page.component.html',
  styleUrls: ['./miracle-form-page.component.scss'],
  standalone: true,
  imports: [
    MatMenuModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    FormsModule,
    MatSelectModule,
    RouterModule,
    RomanPipe,
    CommonModule,
    CountryCodePipe,
    MarkdownComponent,
  ],
  providers: [provideMarkdown()],
})
export class MiracleFormPageComponent implements OnInit, AfterViewInit {
  private miraclesService = inject(MiraclesService);
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
  form!: FormGroup;
  isEditMode = false;
  miracleId: string | null = null;
  imageLoading = false;

  centuries = Array.from({ length: 20 }, (_, i) => i + 1);

  ngOnInit(): void {
    const filter = new EntityFilters({ type: TagType.Miracle });
    filter.pageSize = 100;
    this.tagsService.getTags(filter).subscribe((res) => {
      this.tagsList = res.items;
      this.cdr.detectChanges();
    });

    this.form = new FormBuilder().group({
      title: ['', [Validators.required, notOnlyNumbersValidator()]],
      country: ['', Validators.required],
      century: [null, Validators.required],
      image: ['', Validators.required],
      description: ['', Validators.required],
      markdownContent: ['', Validators.required],
      date: [''],
      location: [''],
    });

    this.route.paramMap.subscribe((params) => {
      this.miracleId = params.get('id');
      this.isEditMode = !!this.miracleId;

      if (this.isEditMode && this.miracleId) {
        this.miraclesService.getMiracleWithMarkdown(this.miracleId).subscribe({
          next: ({ miracle, markdown }) => {
            this.currentTags = miracle.tags.map((tag) => tag.name);
            this.form.patchValue({
              title: miracle.title,
              country: miracle.country,
              century: miracle.century,
              image: miracle.image,
              description: miracle.description,
              markdownContent: markdown,
              date: miracle.date,
              location: miracle.locationDetails,
            });
            setTimeout(() => this.autoResizeOnLoad());
            this.cdr.detectChanges();
          },
          error: () => {
            this.snackBarService.error('Error loading miracle for update');
            this.router.navigate(['admin/miracles']);
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

  onSubmit() {
    if (this.imageLoading) return;

    const tagIds: number[] = this.currentTags
      .map((tagName) => this.tagsList.find((t) => t.name === tagName))
      .filter((t): t is Tag => !!t)
      .map((t) => t.id);

    const miracleData = {
      title: this.form.value.title,
      country: this.form.value.country,
      century: +this.form.value.century,
      image: this.form.value.image,
      description: this.form.value.description,
      markdownContent: this.form.value.markdownContent,
      tagIds,
      date: this.form.value.date,
      locationDetails: this.form.value.location,
    };

    if (this.isEditMode && this.miracleId) {
      this.miraclesService
        .updateMiracle(this.miracleId, miracleData)
        .subscribe({
          next: () => {
            this.snackBarService.success('Miracle successfully updated');
            this.router.navigate(['admin/miracles']);
          },
          error: () => {
            this.snackBarService.error('Error updating miracle');
          },
        });
    } else {
      this.miraclesService.createMiracle(miracleData).subscribe({
        next: () => {
          this.snackBarService.success('Miracle successfully created');
          this.router.navigate(['admin/miracles']);
        },
        error: (err) => {
          const errorMessage =
            typeof err.error === 'string'
              ? err.error
              : (err.error?.message ?? 'Unexpected error.');
          this.snackBarService.error('Error creating miracle: ' + errorMessage);
        },
      });
    }
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
