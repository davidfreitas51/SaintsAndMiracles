import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function matchValueValidator(
  controlName: string,
  matchingControlName: string,
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const control = group.get(controlName);
    const matchingControl = group.get(matchingControlName);

    if (!control || !matchingControl) return null;

    if (control.value !== matchingControl.value) {
      matchingControl.setErrors({ mismatch: true });
      return { mismatch: true };
    }

    if (matchingControl.hasError('mismatch')) {
      matchingControl.setErrors(null);
    }

    return null;
  };
}
