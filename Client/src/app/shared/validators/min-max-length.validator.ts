import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function minMaxLengthValidator(min: number, max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value ?? '';
    if (!value) return null;
    if (value.length < min)
      return {
        minMaxLength: {
          reason: 'min',
          requiredLength: min,
          actualLength: value.length,
        },
      };
    if (value.length > max)
      return {
        minMaxLength: {
          reason: 'max',
          requiredLength: max,
          actualLength: value.length,
        },
      };
    return null;
  };
}
