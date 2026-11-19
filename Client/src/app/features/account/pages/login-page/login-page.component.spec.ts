import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi, type Mocked } from 'vitest';
import { of, throwError } from 'rxjs';

import { LoginPageComponent } from './login-page.component';
import { AuthenticationService } from '../../../../core/services/authentication.service';
import { RegistrationService } from '../../../../core/services/registration.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { createCurrentUser } from '../../../../../testing/test-fixtures';

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let authService: Mocked<AuthenticationService>;
  let registrationService: Mocked<RegistrationService>;
  let snackbar: Mocked<SnackbarService>;
  let router: Mocked<Router>;

  beforeEach(() => {
    authService = {
      login: vi.fn(),
    } as unknown as Mocked<AuthenticationService>;

    registrationService = {
      resendConfirmation: vi.fn(),
    } as unknown as Mocked<RegistrationService>;

    snackbar = {
      error: vi.fn(),
    } as unknown as Mocked<SnackbarService>;

    router = {
      navigate: vi.fn(),
      events: of(),
    } as unknown as Mocked<Router>;

    authService.login.mockReturnValue(of(createCurrentUser()));
    registrationService.resendConfirmation.mockReturnValue(of(void 0));

    TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        { provide: AuthenticationService, useValue: authService },
        { provide: RegistrationService, useValue: registrationService },
        { provide: SnackbarService, useValue: snackbar },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    });

    component = TestBed.createComponent(LoginPageComponent).componentInstance;
  });

  it('does not submit when form is invalid', () => {
    component.onSubmit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.form.controls.email.touched).toBe(true);
    expect(component.form.controls.password.touched).toBe(true);
  });

  it('navigates on successful login', () => {
    component.form.setValue({
      email: 'user@test.com',
      password: 'valid-password',
      rememberMe: false,
    });

    component.onSubmit();

    expect(authService.login).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
    expect(component.isSubmitting).toBe(false);
  });

  it('resends confirmation on unconfirmed email', () => {
    authService.login.mockReturnValue(
      throwError(() => ({ error: 'Email not confirmed' })),
    );

    component.form.setValue({
      email: 'user@test.com',
      password: 'valid-password',
      rememberMe: false,
    });

    component.onSubmit();

    expect(registrationService.resendConfirmation).toHaveBeenCalledWith(
      'user@test.com',
    );
    expect(snackbar.error).toHaveBeenCalledWith(
      'Your email is not confirmed. A new confirmation email has been sent.',
    );
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
