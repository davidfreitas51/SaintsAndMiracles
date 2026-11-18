import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';

import { EmailConfirmedPageComponent } from './email-confirmed-page.component';

describe('EmailConfirmedPageComponent', () => {
  let component: EmailConfirmedPageComponent;
  let fixture: ComponentFixture<EmailConfirmedPageComponent>;

  beforeEach(() => {
    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    );
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailConfirmedPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            queryParams: of({ success: 'true', logout: 'false' }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EmailConfirmedPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
