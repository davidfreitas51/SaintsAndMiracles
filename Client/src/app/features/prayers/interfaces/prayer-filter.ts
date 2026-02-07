import { PrayerOrderBy } from '../enums/prayerOrderBy';

export class PrayerFilters {
  orderBy: PrayerOrderBy = PrayerOrderBy.Title;
  search: string = '';
  tagIds: number[] = [];
  pageNumber: number = 1;
  pageSize: number = 25;
}
