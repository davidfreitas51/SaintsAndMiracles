import { Component, inject, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { DashboardService } from '../../../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  imports: [MatCardModule, MatTableModule],
})
export class DashboardPageComponent implements OnInit {
  dashboardService = inject(DashboardService);

  summary = [
    { label: 'Total Saints', value: 0 },
    { label: 'Total Miracles', value: 0 },
    { label: 'Total Prayers', value: 0 },
    { label: 'Total Accounts', value: 0 },
  ];

  tableColumns: string[] = ['name', 'status', 'lastActive'];
  tableData = [
    { name: 'John Doe', status: 'Active', lastActive: '2 days ago' },
    { name: 'Jane Smith', status: 'Inactive', lastActive: '10 days ago' },
    { name: 'Bob Johnson', status: 'Active', lastActive: '1 day ago' },
  ];

  ngOnInit(): void {
    this.dashboardService.getTotalSaints().subscribe({
      next: (total) => (this.summary[0].value = total),
      error: (err) => console.error('Failed to load total saints:', err),
    });

    this.dashboardService.getTotalMiracles().subscribe({
      next: (total) => (this.summary[1].value = total),
      error: (err) => console.error('Failed to load total miracles:', err),
    });

    this.dashboardService.getTotalPrayers().subscribe({
      next: (total) => (this.summary[2].value = total),
      error: (err) => console.error('Failed to load total prayers', err)
    })

    this.dashboardService.getTotalAccounts().subscribe({
      next: (total) => (this.summary[3].value = total),
      error: (err) => console.error('Failed to load total accounts:', err),
    });
  }
}
