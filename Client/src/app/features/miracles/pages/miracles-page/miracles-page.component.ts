import { Component, inject, OnInit } from '@angular/core';
import { MiraclesService } from '../../../../core/services/miracles.service';
import { MatCardModule } from '@angular/material/card';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Miracle } from '../../interfaces/miracle';
import { environment } from '../../../../../environments/environment';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MiracleFilters } from '../../interfaces/miracle-filter';
import countries from 'i18n-iso-countries';
import enLocale from 'i18n-iso-countries/langs/en.json';
import { CommonModule } from '@angular/common';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { MatDialog } from '@angular/material/dialog';
import { AdvancedSearchMiraclesDialogComponent } from '../../components/advanced-search-miracles-dialog/advanced-search-miracles-dialog.component';
import { Tag } from '../../../../interfaces/tag';
import { MiracleOrderBy } from '../../enums/miracleOrderBy';

countries.registerLocale(enLocale);

@Component({
  selector: 'app-miracles-page',
  standalone: true,
  imports: [
    HeaderComponent,
    FooterComponent,
    MatCardModule,
    RouterLink,
    RomanPipe,
    MatSelectModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    CommonModule,
    CountryCodePipe,
    MatPaginatorModule,
  ],
  templateUrl: './miracles-page.component.html',
  styleUrl: './miracles-page.component.scss',
})
export class MiraclesPageComponent implements OnInit {
  private router = inject(Router);
  private miraclesService = inject(MiraclesService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  public miracles: Miracle[] | null = null;
  MiracleOrderBy = MiracleOrderBy;
  totalCount: number = 0;
  imageBaseUrl = environment.assetsUrl;
  countries: string[] = [];
  centuries: number[] = [];

  miracleFilters: MiracleFilters = new MiracleFilters();
  miracleOrderOptions = [
    { value: MiracleOrderBy.Title, viewValue: 'Title (A-Z)' },
    { value: MiracleOrderBy.TitleDesc, viewValue: 'Title (Z-A)' },
    { value: MiracleOrderBy.Century, viewValue: 'Century (Asc)' },
    { value: MiracleOrderBy.CenturyDesc, viewValue: 'Century (Desc)' },
  ];

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.miracleFilters = new MiracleFilters();

      if (params['country']) this.miracleFilters.country = params['country'];
      if (params['century']) this.miracleFilters.century = params['century'];
      if (params['search']) this.miracleFilters.search = params['search'];
      if (params['orderBy']) this.miracleFilters.orderBy = params['orderBy'];
      if (params['pageNumber'])
        this.miracleFilters.pageNumber = +params['pageNumber'];
      if (params['pageSize'])
        this.miracleFilters.pageSize = +params['pageSize'];
      if (params['tagIds']) {
        const tagIds = (params['tagIds'] as string)
          .split(',')
          .map((id) => Number(id))
          .filter((id) => !isNaN(id));
        this.miracleFilters.tagIds = tagIds;
      }

      this.updateData();
    });
  }

  redirectToMiracleDetails(slug: string) {
    this.router.navigate(['/miracles', slug]);
  }

  private updateData() {
    const queryParams: any = {};

    if (this.miracleFilters.country)
      queryParams.country = this.miracleFilters.country;
    if (this.miracleFilters.century)
      queryParams.century = this.miracleFilters.century;
    if (this.miracleFilters.search)
      queryParams.search = this.miracleFilters.search;
    if (this.miracleFilters.orderBy)
      queryParams.orderBy = this.miracleFilters.orderBy;
    if (this.miracleFilters.pageNumber)
      queryParams.pageNumber = this.miracleFilters.pageNumber;
    if (this.miracleFilters.pageSize)
      queryParams.pageSize = this.miracleFilters.pageSize;
    if (this.miracleFilters.tagIds && this.miracleFilters.tagIds.length > 0) {
      queryParams.tagIds = this.miracleFilters.tagIds.join(',');
    }

    this.router.navigate([], {
      queryParams,
      replaceUrl: true,
    });

    this.miraclesService.getMiracles(this.miracleFilters).subscribe({
      next: (res) => {
        this.miracles = res.items;
        this.totalCount = res.totalCount;
      },
      error: (err) => console.error(err),
    });
  }

  handleFilterChange(value: MiracleOrderBy) {
    this.miracleFilters.orderBy = value;
    this.miracleFilters.pageNumber = 1;
    this.updateData();
  }

  handleSearch(query: string) {
    this.miracleFilters.pageNumber = 1;
    this.miracleFilters.search = query;
    this.updateData();
  }
  clearFilters() {
    this.miracleFilters = new MiracleFilters();
    this.updateData();
  }

  handlePageChange(event: PageEvent): void {
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
      if (result) {
        this.miracleFilters.country = result.country;
        this.miracleFilters.century = result.century
          ? Number(result.century)
          : '';
        this.miracleFilters.tagIds = result.tags.map((t: Tag) => t.id);
        this.miracleFilters.pageNumber = 1;
        this.updateData();
      }
    });
  }
}
