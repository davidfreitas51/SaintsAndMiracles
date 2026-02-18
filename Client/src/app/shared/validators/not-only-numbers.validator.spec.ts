import { FormControl } from '@angular/forms';
import { notOnlyNumbersValidator } from './notOnlyNumbersValidator';

describe('notOnlyNumbersValidator', () => {
  it('returns null for empty values', () => {
    const validator = notOnlyNumbersValidator();
    expect(validator(new FormControl(''))).toBeNull();
  });

  it('returns error for numbers only', () => {
    const validator = notOnlyNumbersValidator();
    expect(validator(new FormControl('12345'))).toEqual({ onlyNumbers: true });
  });

  it('returns null for mixed values', () => {
    const validator = notOnlyNumbersValidator();
    expect(validator(new FormControl('abc123'))).toBeNull();
  });
});
