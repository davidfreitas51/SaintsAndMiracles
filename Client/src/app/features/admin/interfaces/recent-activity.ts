export interface RecentActivity {
  entityName: string;
  entityId: number;
  displayName: string;
  action: 'created' | 'updated' | 'deleted';
  createdAt: string;
  userEmail?: string;
}
