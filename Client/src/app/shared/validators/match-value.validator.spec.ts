import { FormControl, FormGroup } from '@angular/forms';
import { matchValueValidator } from './match-value.validator';

describe('matchValueValidator', () => {
  it('sets mismatch error when values differ', () => {
    const group = new FormGroup({
      password: new FormControl('secret'),
      confirmPassword: new FormControl('other'),
    });

    const validator = matchValueValidator('password', 'confirmPassword');

    expect(validator(group)).toEqual({ mismatch: true });
    expect(group.get('confirmPassword')?.hasError('mismatch')).toBeTrue();
  });

  it('clears mismatch error when values match', () => {
    const group = new FormGroup({
      password: new FormControl('secret'),
      confirmPassword: new FormControl('secret'),
    });

    const validator = matchValueValidator('password', 'confirmPassword');

    expect(validator(group)).toBeNull();
    expect(group.get('confirmPassword')?.hasError('mismatch')).toBeFalse();
  });
});
