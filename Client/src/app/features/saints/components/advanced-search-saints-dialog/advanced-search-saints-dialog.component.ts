import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
  ],
})
export class AdvancedSearchSaintsDialogComponent implements OnInit {
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

  selectedTags: Tag[] = [];
  selectedMonth: string = '';
  selectedCentury: string = '';
  selectedCountry: string = '';
  selectedOrder: string = '';

  ngOnInit(): void {
    const filters = new EntityFilters();
    filters.type = TagType.Saint;

    this.tagsService.getTags(filters).subscribe({
      next: (res) => {
        this.tags = res.items;

        if (this.data.tagIds?.length) {
          const ids = this.data.tagIds.map((t) => t);
          this.selectedTags = this.tags.filter((tag) => ids.includes(tag.id));
        }
      },
      error: (err) => {
        console.error('Failed to load tags', err);
      },
    });

    this.saintsService.getCountries().subscribe({
      next: (res) => {
        this.countries = res;
      },
      error: (err) => {
        console.error('Failed to load countries', err);
      },
    });

    this.religiousOrdersService
      .getOrders(new EntityFilters({ tagType: 'Orders' }))
      .subscribe({
        next: (res) => {
          this.religiousOrders = res.items;
        },
        error: (err) => {
          console.error('Failed to load religious orders', err);
        },
      });

    this.selectedMonth = this.data.feastMonth || '';
    this.selectedOrder = this.data.religiousOrderId || '';
    this.selectedCentury = this.data.century || '';
    this.selectedCountry = this.data.country || '';
  }

  selectTag(tag: Tag) {
    if (!this.selectedTags.some((t) => t.id === tag.id)) {
      this.selectedTags = [...this.selectedTags, tag];
    }
  }

  unselectTag(tag: Tag) {
    this.selectedTags = this.selectedTags.filter((t) => t.id !== tag.id);
  }

  onApplyFilters() {
    this.dialogRef.close({
      century: this.selectedCentury,
      country: this.selectedCountry,
      feastMonth: this.selectedMonth,
      order: this.selectedOrder,
      tags: this.selectedTags,
    });
  }
}
