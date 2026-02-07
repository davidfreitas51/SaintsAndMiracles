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
    FormsModule,
    MatSelectModule,
    RouterModule,
    CommonModule,
    MarkdownComponent,
  ],
  providers: [provideMarkdown()],
})
export class PrayerFormPageComponent implements OnInit, AfterViewInit {
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
  form!: FormGroup;
  isEditMode = false;
  prayerId: string | null = null;
  imageLoading = false;

  ngOnInit(): void {
    const filter = new EntityFilters({ type: TagType.Prayer });
    filter.pageSize = 100;
    this.tagsService.getTags(filter).subscribe((res) => {
      this.tagsList = res.items;
      this.cdr.detectChanges();
    });

    this.form = new FormBuilder().group({
      title: ['', [Validators.required, notOnlyNumbersValidator()]],
      description: ['', Validators.required],
      markdownContent: ['', Validators.required],
      image: [''],
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

  onSubmit() {
    if (this.imageLoading) return;

    const tagIds: number[] = this.currentTags
      .map((tagName) => this.tagsList.find((t) => t.name === tagName))
      .filter((t): t is Tag => !!t)
      .map((t) => t.id);

    const prayerData = {
      title: this.form.value.title,
      description: this.form.value.description,
      markdownContent: this.form.value.markdownContent,
      image: this.form.value.image,
      tagIds,
    };

    if (this.isEditMode && this.prayerId) {
      this.prayersService.updatePrayer(this.prayerId, prayerData).subscribe({
        next: () => {
          this.snackBarService.success('Prayer successfully updated');
          this.router.navigate(['admin/prayers']);
        },
        error: () => {
          this.snackBarService.error('Error updating prayer');
        },
      });
    } else {
      this.prayersService.createPrayer(prayerData).subscribe({
        next: () => {
          this.snackBarService.success('Prayer successfully created');
          this.router.navigate(['admin/prayers']);
        },
        error: (err) => {
          const errorMessage =
            typeof err.error === 'string'
              ? err.error
              : (err.error?.message ?? 'Unexpected error.');
          this.snackBarService.error('Error creating prayer: ' + errorMessage);
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
