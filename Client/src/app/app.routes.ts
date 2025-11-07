import { Routes } from '@angular/router';
import { AboutUsPageComponent } from './pages/about-us-page/about-us-page.component';
import { HomePageComponent } from './pages/home-page/home-page.component';

import { MiracleDetailsPageComponent } from './features/miracles/pages/miracle-details-page/miracle-details-page.component';
import { MiraclesPageComponent } from './features/miracles/pages/miracles-page/miracles-page.component';

import { SaintDetailsPageComponent } from './features/saints/pages/saint-details-page/saint-details-page.component';
import { SaintsPageComponent } from './features/saints/pages/saints-page/saints-page.component';


import { PrayersPageComponent } from './features/prayers/pages/prayers-page/prayers-page.component';
import { PrayerDetailsPageComponent } from './features/prayers/pages/prayer-details-page/prayer-details-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomePageComponent,
  },
  {
    path: 'about',
    component: AboutUsPageComponent,
  },
  {
    path: 'saints',
    component: SaintsPageComponent,
  },
  {
    path: 'saints/:slug',
    component: SaintDetailsPageComponent,
  },
  {
    path: 'miracles',
    component: MiraclesPageComponent,
  },
  {
    path: 'miracles/:slug',
    component: MiracleDetailsPageComponent,
  },
  {
    path: 'prayers',
    component: PrayersPageComponent,
  },
  {
    path: 'prayers/:slug',
    component: PrayerDetailsPageComponent,
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin-routes').then(r => r.adminRoutes)
  },
  {
    path: 'account',
    loadChildren: () => import('./features/account/account-routes').then(r => r.accountRoutes)
  }
];
