import { Entity } from './entity';

export interface PaginatedEntityResponse {
  items: Entity[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
