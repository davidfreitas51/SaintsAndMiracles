import { Component, inject, OnInit } from '@angular/core';
import { MiraclesService } from '../../../../core/services/miracles.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Miracle } from '../../interfaces/miracle';
import { MiracleFilters } from '../../interfaces/miracle-filter';
import { Tag } from '../../../../interfaces/tag';
import { MiracleOrderBy } from '../../enums/miracleOrderBy';
import { environment } from '../../../../../environments/environment';
import { AdvancedSearchMiraclesDialogComponent } from '../../components/advanced-search-miracles-dialog/advanced-search-miracles-dialog.component';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule } from '@angular/forms';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { FormBuilder, FormGroup } from '@angular/forms';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { minMaxLengthValidator } from '../../../../shared/validators/min-max-length.validator';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-miracles-page',
  standalone: true,
  templateUrl: './miracles-page.component.html',
  styleUrls: ['./miracles-page.component.scss'],
  imports: [
    CountryCodePipe,
    MatPaginatorModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    ReactiveFormsModule,
    RomanPipe,
    HeaderComponent,
    RouterLink,
    FooterComponent,
    EmptyStateComponent,
  ],
})
export class MiraclesPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private miraclesService = inject(MiraclesService);
  private dialog = inject(MatDialog);

  miracles: Miracle[] = [];
  totalCount = 0;
  imageBaseUrl = environment.assetsUrl;

  searchForm: FormGroup = this.fb.group({
    search: ['', [minMaxLengthValidator(1, 100)]],
  });

  miracleFilters = new MiracleFilters();
  miracleOrderOptions = [
    { value: MiracleOrderBy.Title, viewValue: 'Title (A-Z)' },
    { value: MiracleOrderBy.TitleDesc, viewValue: 'Title (Z-A)' },
    { value: MiracleOrderBy.Century, viewValue: 'Century (Asc)' },
    { value: MiracleOrderBy.CenturyDesc, viewValue: 'Century (Desc)' },
  ];
  MiracleOrderBy = MiracleOrderBy;

  isLoading = false;

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.loadFiltersFromQueryParams(params);
      this.updateData();
    });
  }

  redirectToMiracleDetails(slug: string) {
    this.router.navigate(['/miracles', slug]);
  }

  private updateData() {
    this.isLoading = true;

    const queryParams = this.buildQueryParams();

    this.router.navigate([], {
      queryParams,
      replaceUrl: true,
    });

    this.miraclesService
      .getMiracles(this.miracleFilters)
      .pipe(
        finalize(() => {
          this.isLoading = false;
        }),
      )
      .subscribe({
        next: (res) => {
          this.miracles = res.items;
          this.totalCount = res.totalCount;
        },
        error: (err) => {
          console.error('Error fetching miracles:', err);
        },
      });
  }

  private loadFiltersFromQueryParams(params: any) {
    const filters = new MiracleFilters();

    filters.country = params['country'] ?? '';
    filters.century = params['century'] ? Number(params['century']) : '';
    filters.search = params['search'] ?? '';
    filters.orderBy = params['orderBy'] ?? MiracleOrderBy.Title;
    filters.pageNumber = params['pageNumber']
      ? Number(params['pageNumber'])
      : 1;
    filters.pageSize = params['pageSize'] ? Number(params['pageSize']) : 25;

    if (params['tagIds']) {
      filters.tagIds = (params['tagIds'] as string)
        .split(',')
        .map((id) => Number(id))
        .filter((id) => !isNaN(id));
    }

    this.miracleFilters = filters;
    this.searchForm.patchValue({ search: filters.search ?? '' });
  }

  private buildQueryParams() {
    const params: any = {};

    if (this.miracleFilters.country)
      params.country = this.miracleFilters.country;
    if (this.miracleFilters.century)
      params.century = this.miracleFilters.century;
    if (this.miracleFilters.search) params.search = this.miracleFilters.search;
    if (this.miracleFilters.orderBy)
      params.orderBy = this.miracleFilters.orderBy;
    if (this.miracleFilters.pageNumber)
      params.pageNumber = this.miracleFilters.pageNumber;
    if (this.miracleFilters.pageSize)
      params.pageSize = this.miracleFilters.pageSize;
    if (this.miracleFilters.tagIds?.length)
      params.tagIds = this.miracleFilters.tagIds.join(',');

    return params;
  }

  handleFilterChange(value: MiracleOrderBy) {
    this.miracleFilters.orderBy = value;
    this.miracleFilters.pageNumber = 1;
    this.updateData();
  }

  handleSearch(query: string) {
    this.miracleFilters.search = query;
    this.miracleFilters.pageNumber = 1;
    this.updateData();
  }

  clearFilters() {
    this.miracleFilters = new MiracleFilters();
    this.searchForm.patchValue({ search: '' });
    this.updateData();
  }

  handlePageChange(event: { pageIndex: number; pageSize: number }) {
    this.miracleFilters.pageNumber = event.pageIndex + 1;
    this.miracleFilters.pageSize = event.pageSize;
    this.updateData();
  }

  handleAdvancedSearch() {
    const dialogRef = this.dialog.open(AdvancedSearchMiraclesDialogComponent, {
      height: '600px',
      width: '600px',
      data: this.miracleFilters,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) return;

      this.miracleFilters.country = result.country ?? '';
      this.miracleFilters.century = result.century
        ? Number(result.century)
        : '';
      this.miracleFilters.tagIds = (result.tags ?? []).map((t: Tag) => t.id);
      this.miracleFilters.pageNumber = 1;
      this.updateData();
    });
  }
}
