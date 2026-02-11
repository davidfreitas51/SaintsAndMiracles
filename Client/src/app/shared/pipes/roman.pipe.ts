import { Pipe, PipeTransform } from '@angular/core';
import { toRoman } from 'roman-numerals';

@Pipe({
  name: 'roman',
})
export class RomanPipe implements PipeTransform {
  transform(value: number): string {
    if (typeof value !== 'number' || isNaN(value)) return '';
    return toRoman(value);
  }
}
