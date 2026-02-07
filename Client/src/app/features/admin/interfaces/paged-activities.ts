import { RecentActivity } from './recent-activity';

export interface PagedRecentActivity {
  items: RecentActivity[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
