import { Routes } from '@angular/router';
import { AboutUsPageComponent } from './pages/about-us-page/about-us-page.component';
import { AdminPageComponent } from './features/admin/pages/admin-page/admin-page.component';
import { LandingPageComponent } from './pages/landing-page/landing-page.component';

import { MiracleDetailsPageComponent } from './features/miracles/pages/miracle-details-page/miracle-details-page.component';
import { MiraclesPageComponent } from './features/miracles/pages/miracles-page/miracles-page.component';
import { MiracleFormPageComponent } from './features/miracles/pages/miracle-form-page/miracle-form-page.component';
import { ManageMiraclesPageComponent } from './features/admin/pages/manage-miracles-page/manage-miracles-page.component';

import { SaintDetailsPageComponent } from './features/saints/pages/saint-details-page/saint-details-page.component';
import { SaintsPageComponent } from './features/saints/pages/saints-page/saints-page.component';
import { SaintFormPageComponent } from './features/saints/pages/saint-form-page/saint-form-page.component';
import { ManageSaintsPageComponent } from './features/admin/pages/manage-saints-page/manage-saints-page.component';

import { DashboardPageComponent } from './features/admin/pages/dashboard-page/dashboard-page.component';

import { PrayersPageComponent } from './features/prayers/pages/prayers-page/prayers-page.component';
import { PrayerDetailsPageComponent } from './features/prayers/pages/prayer-details-page/prayer-details-page.component';
import { PrayerFormPageComponent } from './features/prayers/pages/prayer-form-page/prayer-form-page.component';
import { ManagePrayersPageComponent } from './features/admin/pages/manage-prayers-page/manage-prayers-page.component';
import { LoginPageComponent } from './features/account/pages/login-page/login-page.component';
import { RegisterPageComponent } from './features/account/pages/register-page/register-page.component';
import { EmailConfirmedPageComponent } from './features/account/pages/email-confirmed-page/email-confirmed-page.component';
import { ManageAccountsPageComponent } from './features/admin/pages/manage-accounts-page/manage-accounts-page.component';
import { AuthGuard } from './core/guards/auth.guard';
import { ForgotPasswordPageComponent } from './features/account/pages/forgot-password-page/forgot-password-page.component';
import { ResetPasswordPageComponent } from './features/account/pages/reset-password-page/reset-password-page.component';
import { AccountSettingsPageComponent } from './features/account/pages/account-settings-page/account-settings-page.component';

export const routes: Routes = [
  {
    path: '',
    component: LandingPageComponent,
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
        children: [{ path: '', component: ManageAccountsPageComponent }],
      },
    ],
  },
  {
    path: 'account',
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      },
      {
        path: 'login',
        component: LoginPageComponent,
      },
      {
        path: 'register',
        component: RegisterPageComponent,
      },
      {
        path: 'email-confirmed',
        component: EmailConfirmedPageComponent,
      },
      {
        path: 'forgot-password',
        component: ForgotPasswordPageComponent
      },
      {
        path: 'reset-password',
        component: ResetPasswordPageComponent
      },
      {
        path: 'settings',
        component: AccountSettingsPageComponent,
        canActivate: [AuthGuard]
      }
    ],
  },
];
