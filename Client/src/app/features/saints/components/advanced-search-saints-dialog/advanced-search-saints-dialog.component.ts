import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { TagsService } from '../../../../core/services/tags.service';
import { ReligiousOrdersService } from '../../../../core/services/religious-orders.service';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { Tag } from '../../../../interfaces/tag';
import { ReligiousOrder } from '../../../../interfaces/religious-order';
import { CommonModule } from '@angular/common';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { SaintsService } from '../../../../core/services/saints.service';
import { SaintFilters } from '../../interfaces/saint-filter';

@Component({
  selector: 'app-advanced-search-saints-dialog',
  templateUrl: './advanced-search-saints-dialog.component.html',
  styleUrl: './advanced-search-saints-dialog.component.scss',
  standalone: true,
  imports: [
    MatDialogModule,
    MatSelectModule,
    FormsModule,
    MatButtonModule,
    CommonModule,
    RomanPipe,
    ReactiveFormsModule,
  ],
})
export class AdvancedSearchSaintsDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  readonly dialogRef = inject(
    MatDialogRef<AdvancedSearchSaintsDialogComponent>,
  );
  readonly tagsService = inject(TagsService);
  readonly religiousOrdersService = inject(ReligiousOrdersService);
  readonly saintsService = inject(SaintsService);
  readonly data = inject(MAT_DIALOG_DATA) as SaintFilters;

  months = [
    { value: '1', label: 'January' },
    { value: '2', label: 'February' },
    { value: '3', label: 'March' },
    { value: '4', label: 'April' },
    { value: '5', label: 'May' },
    { value: '6', label: 'June' },
    { value: '7', label: 'July' },
    { value: '8', label: 'August' },
    { value: '9', label: 'September' },
    { value: '10', label: 'October' },
    { value: '11', label: 'November' },
    { value: '12', label: 'December' },
  ];

  tags: Tag[] = [];
  religiousOrders: ReligiousOrder[] = [];

  countries: string[] = [];
  centuries: number[] = Array.from({ length: 21 }, (_, i) => i + 1);

  form = this.fb.nonNullable.group({
    country: [''],
    century: ['' as number | ''],
    month: [''],
    order: ['' as number | ''],
    tags: [[] as Tag[]],
  });

  ngOnInit(): void {
    this.form.patchValue({
      country: this.data.country || '',
      month: this.data.feastMonth || '',
      century: this.data.century ? Number(this.data.century) : '',
      tags: [],
      order: '',
    });

    const tagFilters = new EntityFilters({ type: TagType.Saint });
    this.tagsService.getTags(tagFilters).subscribe({
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

    this.saintsService.getCountries().subscribe({
      next: (res) => (this.countries = res),
      error: (err) => console.error('Failed to load countries', err),
    });

    this.religiousOrdersService
      .getOrders(new EntityFilters({ tagType: 'Orders' }))
      .subscribe({
        next: (res) => {
          this.religiousOrders = res.items;

          if (this.data.religiousOrderId != null) {
            this.form.controls.order.setValue(
              Number(this.data.religiousOrderId),
            );
          }
        },
        error: (err) => console.error('Failed to load religious orders', err),
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
      feastMonth: value.month,
      order: value.order,
      tags: value.tags,
    });
  }
}
