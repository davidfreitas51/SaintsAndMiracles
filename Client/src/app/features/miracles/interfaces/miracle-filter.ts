import { MiracleOrderBy } from "../enums/miracleOrderBy";

export class MiracleFilters {
  orderBy: MiracleOrderBy = MiracleOrderBy.Title;
  country: string = '';
  century: number | '' = '';
  search: string = '';
  tagIds: number[] = [];
  pageNumber: number = 1;
  pageSize: number = 25;
}
