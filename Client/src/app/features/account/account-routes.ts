import { Route } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { AccountSettingsPageComponent } from './pages/account-settings-page/account-settings-page.component';
import { EmailConfirmedPageComponent } from './pages/email-confirmed-page/email-confirmed-page.component';
import { ForgotPasswordPageComponent } from './pages/forgot-password-page/forgot-password-page.component';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { RegisterPageComponent } from './pages/register-page/register-page.component';
import { RegistrationConfirmationPageComponent } from './pages/registration-confirmation-page/registration-confirmation-page.component';
import { ResetPasswordPageComponent } from './pages/reset-password-page/reset-password-page.component';

export const accountRoutes: Route[] = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
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
        path: 'registration-confirmation',
        component: RegistrationConfirmationPageComponent,
      },
      {
        path: 'forgot-password',
        component: ForgotPasswordPageComponent,
      },
      {
        path: 'reset-password',
        component: ResetPasswordPageComponent,
      },
      {
        path: 'settings',
        component: AccountSettingsPageComponent,
        canActivate: [AuthGuard],
      },
    ],
  },
];
