export interface RecentActivity {
  name: string; 
  type: string; 
  date: string; 
  action: 'created' | 'updated' | 'deleted';
  userEmail?: string; 
}
