import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function feastDayValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const raw = control.value ?? '';

    const value = raw.replace(/_/g, '').trim();

    if (!value || value.length < 5) return null;

    const match = /^(\d{2})\/(\d{2})$/.exec(value);
    if (!match) return { feastDay: { reason: 'format' } };

    const day = Number(match[1]);
    const month = Number(match[2]);

    if (month < 1 || month > 12) {
      return { feastDay: { reason: 'month' } };
    }

    if (day < 1 || day > 31) {
      return { feastDay: { reason: 'day' } };
    }

    return null;
  };
}
