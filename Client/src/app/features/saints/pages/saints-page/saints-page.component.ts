import { Component, inject, OnInit } from '@angular/core';
import { SaintsService } from '../../../../core/services/saints.service';
import { MatCardModule } from '@angular/material/card';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Saint } from '../../interfaces/saint';
import { environment } from '../../../../../environments/environment';
import { RomanPipe } from '../../../../shared/pipes/roman.pipe';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SaintFilters } from '../../interfaces/saint-filter';
import countries from 'i18n-iso-countries';
import enLocale from 'i18n-iso-countries/langs/en.json';
import { CommonModule } from '@angular/common';
import { CountryCodePipe } from '../../../../shared/pipes/country-code.pipe';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { MatDialog } from '@angular/material/dialog';
import { AdvancedSearchSaintsDialogComponent } from '../../components/advanced-search-saints-dialog/advanced-search-saints-dialog.component';
import { Tag } from '../../../../interfaces/tag';
import { SaintOrderBy } from '../../enums/saintOrderBy';
countries.registerLocale(enLocale);

@Component({
  selector: 'app-saints-page',
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
    MatPaginator,
  ],
  templateUrl: './saints-page.component.html',
  styleUrl: './saints-page.component.scss',
})
export class SaintsPageComponent implements OnInit {
  private router = inject(Router);
  private saintsService = inject(SaintsService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);
  advancedFiltersOpen = false;

  months: string[] = [];
  religiousOrders: string[] = [];
  public saints: Saint[] | null = null;
  SaintOrderBy = SaintOrderBy
  totalCount: number = 0;
  imageBaseUrl = environment.assetsUrl;

  saintFilters: SaintFilters = new SaintFilters();
  saintOrderOptions = [
    { value: SaintOrderBy.Name, viewValue: 'Name (A-Z)' },
    { value: SaintOrderBy.NameDesc, viewValue: 'Name (Z-A)' },
    { value: SaintOrderBy.Century, viewValue: 'Century (Asc)' },
    { value: SaintOrderBy.CenturyDesc, viewValue: 'Century (Desc)' },
    { value: SaintOrderBy.FeastDay, viewValue: 'Feast Day (Asc)' },
    { value: SaintOrderBy.FeastDayDesc, viewValue: 'Feast Day (Desc)' },
  ];

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.saintFilters = new SaintFilters();

      if (params['country']) this.saintFilters.country = params['country'];
      if (params['century']) this.saintFilters.century = params['century'];
      if (params['search']) this.saintFilters.search = params['search'];
      if (params['feastMonth'])
        this.saintFilters.feastMonth = params['feastMonth'];
      if (params['religiousOrderId'])
        this.saintFilters.religiousOrderId = params['religiousOrderId'];
      if (params['orderBy']) this.saintFilters.orderBy = params['orderBy'];
      if (params['pageNumber'])
        this.saintFilters.pageNumber = +params['pageNumber'];
      if (params['pageSize']) this.saintFilters.pageSize = +params['pageSize'];

      if (params['tagIds']) {
        const tagIds = (params['tagIds'] as string)
          .split(',')
          .map((id) => Number(id))
          .filter((id) => !isNaN(id));
        this.saintFilters.tagIds = tagIds;
      }

      this.updateData();
    });
  }

  redirectToSaintDetails(slug: string) {
    this.router.navigate(['/saints', slug]);
  }

  private updateData() {
    const queryParams: any = {};

    if (this.saintFilters.country)
      queryParams.country = this.saintFilters.country;
    if (this.saintFilters.century)
      queryParams.century = this.saintFilters.century;
    if (this.saintFilters.search) queryParams.search = this.saintFilters.search;
    if (this.saintFilters.feastMonth)
      queryParams.feastMonth = this.saintFilters.feastMonth;
    if (this.saintFilters.religiousOrderId)
      queryParams.religiousOrderId = this.saintFilters.religiousOrderId;
    if (this.saintFilters.orderBy)
      queryParams.orderBy = this.saintFilters.orderBy;
    if (this.saintFilters.pageNumber)
      queryParams.pageNumber = this.saintFilters.pageNumber;
    if (this.saintFilters.pageSize)
      queryParams.pageSize = this.saintFilters.pageSize;
    if (this.saintFilters.tagIds && this.saintFilters.tagIds.length > 0) {
      queryParams.tagIds = this.saintFilters.tagIds.join(',');
    }

    this.router.navigate([], {
      queryParams,
      replaceUrl: true,
    });

    this.saintsService.getSaints(this.saintFilters).subscribe({
      next: (res) => {
        this.saints = res.items;
        this.totalCount = res.totalCount;
      },
      error: (err) => console.error(err),
    });
  }

  handleFilterChange(value: SaintOrderBy) {
    this.saintFilters.orderBy = value;
    this.saintFilters.pageNumber = 1;
    this.updateData();
  }

  handleSearch(query: string) {
    this.saintFilters.pageNumber = 1;
    this.saintFilters.search = query;
    this.updateData();
  }

  clearFilters() {
    this.saintFilters = new SaintFilters();
    this.updateData();
  }

  handlePageChange(event: PageEvent): void {
    this.saintFilters.pageNumber = event.pageIndex + 1;
    this.saintFilters.pageSize = event.pageSize;
    this.updateData();
  }

  handleAdvancedSearch() {
    const dialogRef = this.dialog.open(AdvancedSearchSaintsDialogComponent, {
      height: '600px',
      width: '600px',
      data: this.saintFilters,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.saintFilters.country = result.country;
        this.saintFilters.century = result.century;
        this.saintFilters.feastMonth = result.feastMonth;
        this.saintFilters.religiousOrderId = result.order;
        this.saintFilters.tagIds = result.tags.map((t: Tag) => t.id);
        this.saintFilters.pageNumber = 1; 
        this.updateData();
      }
    });
  }
}
