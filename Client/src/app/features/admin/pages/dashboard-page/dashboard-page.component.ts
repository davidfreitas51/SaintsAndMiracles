import {
  Component,
  inject,
  OnInit,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

import { DashboardService } from '../../../../core/services/dashboard.service';
import { RecentActivity } from '../../interfaces/recent-activity';
import { DashboardSummaryDto } from '../../interfaces/dashboard-summary-dto';

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

  tableColumns: string[] = ['name', 'type', 'date', 'action'];
  dataSource = new MatTableDataSource<RecentActivity>([
    {
      name: 'Saint Teresa',
      type: 'Saint',
      date: '2025-09-01',
      action: 'updated',
    },
    {
      name: 'Healing Miracle',
      type: 'Miracle',
      date: '2025-09-02',
      action: 'created',
    },
    {
      name: 'Morning Prayer',
      type: 'Prayer',
      date: '2025-09-03',
      action: 'deleted',
    },
  ]);

  ngOnInit(): void {
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

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }
}
