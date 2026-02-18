import { FormControl } from '@angular/forms';
import { personNameValidator } from './person-name.validator';

describe('personNameValidator', () => {
  it('returns null for empty values', () => {
    expect(personNameValidator(new FormControl(''))).toBeNull();
  });

  it('returns error for non-string values', () => {
    expect(personNameValidator(new FormControl(123))).toEqual({
      personName: { reason: 'mustBeString' },
    });
  });

  it('returns error for invalid length', () => {
    expect(personNameValidator(new FormControl('A'))).toEqual({
      personName: { reason: 'length', minLength: 2, maxLength: 100 },
    });
  });

  it('returns null for valid names', () => {
    expect(personNameValidator(new FormControl('John Doe'))).toBeNull();
  });
});
