import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { TagsService } from '../../../../core/services/tags.service';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { Tag } from '../../../../interfaces/tag';
import { MiracleFilters } from '../../interfaces/miracle-filter';
import { Saint } from '../../../saints/interfaces/saint';
import { MiraclesService } from '../../../../core/services/miracles.service';

@Component({
  selector: 'app-advanced-search-miracles-dialog',
  templateUrl: './advanced-search-miracles-dialog.component.html',
  styleUrl: './advanced-search-miracles-dialog.component.scss',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatSelectModule,
    MatButtonModule,
    RomanPipe,
  ],
})
export class AdvancedSearchMiraclesDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(
    MatDialogRef<AdvancedSearchMiraclesDialogComponent>,
  );
  private tagsService = inject(TagsService);
  private miraclesService = inject(MiraclesService);
  private data = inject(MAT_DIALOG_DATA) as MiracleFilters;

  tags: Tag[] = [];
  saints: Saint[] = [];
  countries: string[] = [];
  centuries: number[] = Array.from({ length: 21 }, (_, i) => i + 1);

  form = this.fb.nonNullable.group({
    century: ['' as number | ''],
    country: [''],
    tags: [[] as Tag[]],
  });

  ngOnInit(): void {
    // preload values from dialog data
    this.form.patchValue({
      century: this.data.century ? Number(this.data.century) : '',
      country: this.data.country || '',
    });

    this.tagsService
      .getTags(new EntityFilters({ type: TagType.Miracle }))
      .subscribe({
        next: (res) => {
          this.tags = res.items;

          if (this.data.tagIds?.length) {
            const selected = this.tags.filter((t) =>
              this.data.tagIds!.includes(t.id),
            );

            this.form.controls.tags.setValue(selected);
          }
        },
        error: (err) => console.error('Failed to load tags', err),
      });

    this.miraclesService.getCountries().subscribe({
      next: (res) => (this.countries = res),
      error: (err) => console.error('Failed to load countries', err),
    });
  }

  selectTag(tag: Tag) {
    const current = this.form.controls.tags.value;

    if (!current.some((t) => t.id === tag.id)) {
      this.form.controls.tags.setValue([...current, tag]);
    }
  }

  unselectTag(tag: Tag) {
    const current = this.form.controls.tags.value;

    this.form.controls.tags.setValue(current.filter((t) => t.id !== tag.id));
  }

  onApplyFilters() {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();

    this.dialogRef.close({
      century: value.century,
      country: value.country,
      tags: value.tags,
    });
  }
}
