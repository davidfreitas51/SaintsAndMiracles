import { FormControl } from '@angular/forms';
import { safeEmailValidator } from './safe-email.validator';

describe('safeEmailValidator', () => {
  it('requires a value', () => {
    expect(safeEmailValidator(new FormControl(''))).toEqual({ required: true });
  });

  it('rejects invalid format', () => {
    expect(safeEmailValidator(new FormControl('not-an-email'))).toEqual({
      invalidEmail: true,
    });
  });

  it('rejects unsafe characters', () => {
    expect(safeEmailValidator(new FormControl('bad<@test.com'))).toEqual({
      unsafeChars: true,
    });
  });

  it('returns null for valid email', () => {
    expect(safeEmailValidator(new FormControl('user@test.com'))).toBeNull();
  });
});
