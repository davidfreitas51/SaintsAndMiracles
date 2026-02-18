import { RomanPipe } from './roman.pipe';

describe('RomanPipe', () => {
  it('converts numbers to roman numerals', () => {
    const pipe = new RomanPipe();
    expect(pipe.transform(4)).toBe('IV');
  });

  it('returns empty string for invalid values', () => {
    const pipe = new RomanPipe();
    expect(pipe.transform(NaN)).toBe('');
  });
});
