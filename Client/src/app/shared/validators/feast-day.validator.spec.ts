import { FormControl } from '@angular/forms';
import { feastDayValidator } from './feast-day.validator';

describe('feastDayValidator', () => {
  const validator = feastDayValidator();

  it('returns null for empty or short values', () => {
    expect(validator(new FormControl(''))).toBeNull();
    expect(validator(new FormControl('1/2'))).toBeNull();
  });

  it('returns format error for invalid patterns', () => {
    expect(validator(new FormControl('12-34'))).toEqual({
      feastDay: { reason: 'format' },
    });
  });

  it('returns month error for out-of-range months', () => {
    expect(validator(new FormControl('10/13'))).toEqual({
      feastDay: { reason: 'month' },
    });
  });

  it('returns day error for out-of-range days', () => {
    expect(validator(new FormControl('32/12'))).toEqual({
      feastDay: { reason: 'day' },
    });
  });

  it('returns null for valid values', () => {
    expect(validator(new FormControl('25/12'))).toBeNull();
  });
});
