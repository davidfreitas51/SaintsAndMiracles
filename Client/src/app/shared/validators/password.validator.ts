import { AbstractControl, ValidationErrors } from "@angular/forms";

export function passwordValidator(
  control: AbstractControl,
): ValidationErrors | null {
  const value = control.value as string;

  if (!value) return null;

  if (value.length < 12) {
    return { weakPassword: true };
  }

  return null;
}
