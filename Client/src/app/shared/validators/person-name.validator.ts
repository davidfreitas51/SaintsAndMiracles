import { AbstractControl, ValidationErrors } from '@angular/forms';

export function personNameValidator(
  control: AbstractControl,
): ValidationErrors | null {
  const minLength = 2;
  const maxLength = 100;

  const regex = /^[\p{L}\p{M}'\- ]+$/u;

  const value = control.value;

  if (value == null || value === '') return null;

  if (typeof value !== 'string') {
    return { personName: { reason: 'mustBeString' } };
  }

  const name = value.trim();

  if (name.length < minLength || name.length > maxLength) {
    return {
      personName: {
        reason: 'length',
        minLength,
        maxLength,
      },
    };
  }

  if (!regex.test(name)) {
    return {
      personName: {
        reason: 'invalidFormat',
      },
    };
  }

  return null;
}
