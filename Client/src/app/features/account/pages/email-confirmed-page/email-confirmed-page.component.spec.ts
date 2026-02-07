import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailConfirmedPageComponent } from './email-confirmed-page.component';

describe('EmailConfirmedPageComponent', () => {
  let component: EmailConfirmedPageComponent;
  let fixture: ComponentFixture<EmailConfirmedPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailConfirmedPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmailConfirmedPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
