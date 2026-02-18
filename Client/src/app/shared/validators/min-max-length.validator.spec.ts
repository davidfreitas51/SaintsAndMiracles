import { FormControl } from '@angular/forms';
import { minMaxLengthValidator } from './min-max-length.validator';

describe('minMaxLengthValidator', () => {
  it('returns null for empty values', () => {
    const control = new FormControl('');
    const validator = minMaxLengthValidator(3, 5);

    expect(validator(control)).toBeNull();
  });

  it('returns min error when below min length', () => {
    const control = new FormControl('ab');
    const validator = minMaxLengthValidator(3, 5);

    expect(validator(control)).toEqual({
      minMaxLength: { reason: 'min', requiredLength: 3, actualLength: 2 },
    });
  });

  it('returns max error when above max length', () => {
    const control = new FormControl('abcdef');
    const validator = minMaxLengthValidator(3, 5);

    expect(validator(control)).toEqual({
      minMaxLength: { reason: 'max', requiredLength: 5, actualLength: 6 },
    });
  });

  it('returns null for values within range', () => {
    const control = new FormControl('abcd');
    const validator = minMaxLengthValidator(3, 5);

    expect(validator(control)).toBeNull();
  });
});
