export interface NewPrayerDto {
  title: string;
  description: string;
  image?: string;
  markdownContent: string;
  tagIds?: number[];
}
