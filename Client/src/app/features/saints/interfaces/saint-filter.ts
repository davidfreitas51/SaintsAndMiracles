import { SaintOrderBy } from '../enums/saintOrderBy';

export class SaintFilters {
  orderBy: SaintOrderBy = SaintOrderBy.Name;
  country: string = '';
  century: string = '';
  search: string = '';
  feastMonth: string = '';
  religiousOrderId: string = '';
  tagIds: number[] = [];
  pageNumber: number = 1;
  pageSize: number = 25;
}
