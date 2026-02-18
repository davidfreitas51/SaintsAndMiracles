import { FormControl } from '@angular/forms';
import { passwordValidator } from './password.validator';

describe('passwordValidator', () => {
  it('returns null for empty values', () => {
    expect(passwordValidator(new FormControl(''))).toBeNull();
  });

  it('flags weak passwords under 12 chars', () => {
    expect(passwordValidator(new FormControl('short'))).toEqual({
      weakPassword: true,
    });
  });

  it('returns null for strong passwords', () => {
    expect(passwordValidator(new FormControl('longenoughpwd'))).toBeNull();
  });
});
