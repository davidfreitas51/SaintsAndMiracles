import { Route } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { SuperAdminGuard } from '../../core/guards/super-admin.guard';
import { MiracleFormPageComponent } from '../miracles/pages/miracle-form-page/miracle-form-page.component';
import { PrayerFormPageComponent } from '../prayers/pages/prayer-form-page/prayer-form-page.component';
import { SaintFormPageComponent } from '../saints/pages/saint-form-page/saint-form-page.component';
import { AdminPageComponent } from './pages/admin-page/admin-page.component';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';
import { ManageAccountsPageComponent } from './pages/manage-accounts-page/manage-accounts-page.component';
import { ManageMiraclesPageComponent } from './pages/manage-miracles-page/manage-miracles-page.component';
import { ManagePrayersPageComponent } from './pages/manage-prayers-page/manage-prayers-page.component';
import { ManageSaintsPageComponent } from './pages/manage-saints-page/manage-saints-page.component';

export const adminRoutes: Route[] = [
  {
    path: '',
    canActivate: [AuthGuard],
    component: AdminPageComponent,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: DashboardPageComponent,
      },
      {
        path: 'saints',
        children: [
          { path: '', component: ManageSaintsPageComponent },
          { path: 'create', component: SaintFormPageComponent },
          { path: ':id/edit', component: SaintFormPageComponent },
        ],
      },
      {
        path: 'miracles',
        children: [
          { path: '', component: ManageMiraclesPageComponent },
          { path: 'create', component: MiracleFormPageComponent },
          { path: ':id/edit', component: MiracleFormPageComponent },
        ],
      },
      {
        path: 'prayers',
        children: [
          { path: '', component: ManagePrayersPageComponent },
          { path: 'create', component: PrayerFormPageComponent },
          { path: ':id/edit', component: PrayerFormPageComponent },
        ],
      },
      {
        path: 'accounts',
        canActivate: [SuperAdminGuard],
        children: [{ path: '', component: ManageAccountsPageComponent }],
      },
    ],
  },
];
