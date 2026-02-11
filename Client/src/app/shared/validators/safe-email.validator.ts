import { AbstractControl, ValidationErrors } from '@angular/forms';

const unsafeCharsRegex = /[<>&"']/;

export function safeEmailValidator(
  control: AbstractControl,
): ValidationErrors | null {
  const value = (control.value ?? '').trim();

  if (!value) return { required: true };
  if (value.length > 254)
    return { maxLength: { requiredLength: 254, actualLength: value.length } };
  if (/[^\x20-\x7E]/.test(value)) return { controlChars: true };
  if (value.includes(' ')) return { spaces: true };
  if (unsafeCharsRegex.test(value)) return { unsafeChars: true };

  // Regex simples de email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(value)) return { invalidEmail: true };

  const domain = value.split('@')[1];
  const domainParts = domain.split('.');
  if (domainParts.length < 2 || domainParts.some((p: string) => !p.trim()))
    return { invalidDomain: true };

  return null;
}
