import { FeastDayFormatPipe } from './feast-day-format.pipe';

describe('FeastDayFormatPipe', () => {
  it('formats year 0001 as day/month', () => {
    const pipe = new FeastDayFormatPipe();
    expect(pipe.transform('0001-12-25')).toBe('25/12');
  });

  it('formats other years as day/month/year', () => {
    const pipe = new FeastDayFormatPipe();
    expect(pipe.transform('1999-01-02')).toBe('02/01/1999');
  });

  it('returns the original value when invalid', () => {
    const pipe = new FeastDayFormatPipe();
    expect(pipe.transform('invalid')).toBe('invalid');
  });
});
