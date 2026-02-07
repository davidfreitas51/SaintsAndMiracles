import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { TagsService } from '../../../../core/services/tags.service';
import { SaintsService } from '../../../../core/services/saints.service';
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
    MatDialogModule,
    MatSelectModule,
    FormsModule,
    MatButtonModule,
    CommonModule,
    RomanPipe,
  ],
})
export class AdvancedSearchMiraclesDialogComponent implements OnInit {
  readonly dialogRef = inject(
    MatDialogRef<AdvancedSearchMiraclesDialogComponent>,
  );
  readonly tagsService = inject(TagsService);
  readonly saintsService = inject(SaintsService);
  readonly miraclesService = inject(MiraclesService);
  readonly data = inject(MAT_DIALOG_DATA) as MiracleFilters;

  tags: Tag[] = [];
  saints: Saint[] = [];

  countries: string[] = [];
  centuries: number[] = Array.from({ length: 21 }, (_, i) => i + 1);

  selectedTags: Tag[] = [];
  selectedCentury: number | '' = '';
  selectedCountry: string = '';

  ngOnInit(): void {
    this.tagsService
      .getTags(new EntityFilters({ type: TagType.Miracle }))
      .subscribe({
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

    this.miraclesService.getCountries().subscribe({
      next: (res) => {
        this.countries = res;
      },
      error: (err) => {
        console.error('Failed to load countries', err);
      },
    });

    this.selectedCentury = this.data.century ? Number(this.data.century) : '';
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
      tags: this.selectedTags,
    });
  }
}
