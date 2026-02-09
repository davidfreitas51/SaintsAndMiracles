import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import { PrayersService } from '../../../../core/services/prayers.service';
import { Prayer } from '../../interfaces/prayer';
import { PrayerFilters } from '../../interfaces/prayer-filter';
import { environment } from '../../../../../environments/environment';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { PrayerOrderBy } from '../../enums/prayerOrderBy';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { minMaxLengthValidator } from '../../../../shared/validators/min-max-length.validator';

@Component({
  selector: 'app-prayers-page',
  standalone: true,
  imports: [
    HeaderComponent,
    FooterComponent,
    MatCardModule,
    RouterLink,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    EmptyStateComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './prayers-page.component.html',
  styleUrl: './prayers-page.component.scss',
})
export class PrayersPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private prayersService = inject(PrayersService);
  private route = inject(ActivatedRoute);

  public prayers: Prayer[] = [];
  PrayerOrderBy = PrayerOrderBy;
  totalCount: number = 0;
  imageBaseUrl = environment.assetsUrl;

  searchForm: FormGroup = this.fb.group({
    search: ['', minMaxLengthValidator(1, 100)],
  });

  prayerFilters: PrayerFilters = new PrayerFilters();
  prayerOrderOptions = [
    { value: PrayerOrderBy.Title, viewValue: 'Title (A-Z)' },
    { value: PrayerOrderBy.TitleDesc, viewValue: 'Title (Z-A)' },
  ];

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.prayerFilters = new PrayerFilters();

      if (params['search']) this.prayerFilters.search = params['search'];
      if (params['orderBy']) this.prayerFilters.orderBy = params['orderBy'];
      if (params['pageNumber'])
        this.prayerFilters.pageNumber = +params['pageNumber'];
      if (params['pageSize']) this.prayerFilters.pageSize = +params['pageSize'];
      if (params['tagIds']) {
        const tagIds = (params['tagIds'] as string)
          .split(',')
          .map((id) => Number(id))
          .filter((id) => !isNaN(id));
        this.prayerFilters.tagIds = tagIds;
      }

      const searchValue = params['search'] ?? '';
      this.searchForm.controls['search'].setValue(searchValue);

      this.updateData();
    });
  }

  redirectToPrayerDetails(slug: string) {
    this.router.navigate(['/prayers', slug]);
  }

  private updateData() {
    const queryParams: any = {};

    if (this.prayerFilters.search)
      queryParams.search = this.prayerFilters.search;
    if (this.prayerFilters.orderBy)
      queryParams.orderBy = this.prayerFilters.orderBy;
    if (this.prayerFilters.pageNumber)
      queryParams.pageNumber = this.prayerFilters.pageNumber;
    if (this.prayerFilters.pageSize)
      queryParams.pageSize = this.prayerFilters.pageSize;
    if (this.prayerFilters.tagIds && this.prayerFilters.tagIds.length > 0) {
      queryParams.tagIds = this.prayerFilters.tagIds.join(',');
    }

    this.router.navigate([], {
      queryParams,
      replaceUrl: true,
    });

    this.prayersService.getPrayers(this.prayerFilters).subscribe({
      next: (res) => {
        this.prayers = res.items;
        this.totalCount = res.totalCount;
      },
      error: (err) => console.error(err),
    });
  }

  handleFilterChange(value: PrayerOrderBy) {
    this.prayerFilters.orderBy = value;
    this.prayerFilters.pageNumber = 1;
    this.updateData();
  }

  handleSearch(query: string) {
    this.prayerFilters.pageNumber = 1;
    this.prayerFilters.search = query;
    this.updateData();
  }

  clearFilters() {
    this.prayerFilters = new PrayerFilters();
    this.updateData();
  }

  handlePageChange(event: PageEvent): void {
    this.prayerFilters.pageNumber = event.pageIndex + 1;
    this.prayerFilters.pageSize = event.pageSize;
    this.updateData();
  }
}
