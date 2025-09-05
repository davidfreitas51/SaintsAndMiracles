import {
  Component,
  inject,
  OnInit,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from '@angular/material/paginator';

import { DashboardService } from '../../../../core/services/dashboard.service';
import { DashboardSummaryDto } from '../../interfaces/dashboard-summary-dto';
import { RecentActivity } from '../../interfaces/recent-activity';
import { PagedRecentActivity } from '../../interfaces/paged-activities';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatPaginatorModule],
})
export class DashboardPageComponent implements OnInit, AfterViewInit {
  private dashboardService = inject(DashboardService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  summary = [
    { label: 'Total Saints', value: 0 },
    { label: 'Total Miracles', value: 0 },
    { label: 'Total Prayers', value: 0 },
    { label: 'Total Accounts', value: 0 },
  ];

  tableColumns: string[] = [
    'createdAt',
    'entityName',
    'displayName',
    'action',
    'userEmail',
    'entityId',
  ];

  dataSource = new MatTableDataSource<RecentActivity>([]);
  totalCount = 0;
  pageSize = 10;
  pageNumber = 1;
  isLoading = false;

  ngOnInit(): void {
    this.loadSummary();
    this.loadRecentActivities();
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.paginator.page.subscribe((event: PageEvent) =>
        this.onPageChange(event)
      );
    }
  }

  private loadSummary(): void {
    this.dashboardService.getSummary().subscribe({
      next: (summary: DashboardSummaryDto) => {
        this.summary[0].value = summary.totalSaints;
        this.summary[1].value = summary.totalMiracles;
        this.summary[2].value = summary.totalPrayers;
        this.summary[3].value = summary.totalAccounts;
      },
      error: (err) => console.error('Failed to load dashboard summary:', err),
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadRecentActivities();
  }

  loadRecentActivities(): void {
    this.dashboardService
      .getRecentActivities(this.pageNumber, this.pageSize)
      .subscribe({
        next: (pagedResult) => {
          this.dataSource.data = pagedResult.items;
          this.totalCount = pagedResult.totalCount;
        },
        error: (err) => console.error('Failed to load recent activities:', err),
      });
  }

  getActionClass(action: string): string {
    switch (action.toLowerCase()) {
      case 'created':
        return 'text-green-600 font-semibold';
      case 'updated':
        return 'text-blue-600 font-semibold';
      case 'deleted':
        return 'text-red-600 font-semibold';
      default:
        return '';
    }
  }
}
