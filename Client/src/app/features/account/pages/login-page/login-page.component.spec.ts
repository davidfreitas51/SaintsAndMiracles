import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { LoginPageComponent } from './login-page.component';
import { AuthenticationService } from '../../../../core/services/authentication.service';
import { RegistrationService } from '../../../../core/services/registration.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { createCurrentUser } from '../../../../../testing/test-fixtures';

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let authService: jasmine.SpyObj<AuthenticationService>;
  let registrationService: jasmine.SpyObj<RegistrationService>;
  let snackbar: jasmine.SpyObj<SnackbarService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj<AuthenticationService>('auth', [
      'login',
    ]);
    registrationService = jasmine.createSpyObj<RegistrationService>(
      'registration',
      ['resendConfirmation'],
    );
    snackbar = jasmine.createSpyObj<SnackbarService>('snackbar', ['error']);
    router = jasmine.createSpyObj<Router>('router', ['navigate'], {
      events: of(),
    });

    authService.login.and.returnValue(of(createCurrentUser()));
    registrationService.resendConfirmation.and.returnValue(of(void 0));

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
    }).compileComponents();

    component = TestBed.createComponent(LoginPageComponent).componentInstance;
  });

  it('does not submit when form is invalid', () => {
    authService.login.and.returnValue(of(createCurrentUser()));

    component.onSubmit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.form.controls.email.touched).toBeTrue();
    expect(component.form.controls.password.touched).toBeTrue();
  });

  it('navigates on successful login', () => {
    authService.login.and.returnValue(of(createCurrentUser()));

    component.form.setValue({
      email: 'user@test.com',
      password: 'valid-password',
      rememberMe: false,
    });

    component.onSubmit();

    expect(authService.login).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
    expect(component.isSubmitting).toBeFalse();
  });

  it('resends confirmation on unconfirmed email', () => {
    authService.login.and.returnValue(
      throwError(() => ({ error: 'Email not confirmed' })),
    );
    registrationService.resendConfirmation.and.returnValue(of(void 0));

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
