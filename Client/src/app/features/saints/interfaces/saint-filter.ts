import { SaintOrderBy } from '../enums/saintOrderBy';

export class SaintFilters {
  orderBy: SaintOrderBy = SaintOrderBy.Name;
  country: string = '';
  century?: number;
  search: string = '';
  feastMonth: string = '';
  religiousOrderId?: number;
  tagIds: number[] = [];
  pageNumber: number = 1;
  pageSize: number = 25;
}
