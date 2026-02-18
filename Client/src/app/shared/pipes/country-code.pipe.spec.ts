import { CountryCodePipe } from './country-code.pipe';

describe('CountryCodePipe', () => {
  it('returns a lower-case alpha-2 code for known countries', () => {
    const pipe = new CountryCodePipe();
    expect(pipe.transform('Italy')).toBe('it');
  });

  it('returns null for unknown countries', () => {
    const pipe = new CountryCodePipe();
    expect(pipe.transform('Atlantis')).toBeNull();
  });

  it('returns null for empty values', () => {
    const pipe = new CountryCodePipe();
    expect(pipe.transform(undefined)).toBeNull();
  });
});
