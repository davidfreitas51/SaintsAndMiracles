import { Tag } from '../../../interfaces/tag';

export interface Miracle {
  id: number;
  title: string;
  country: string;
  century: number;
  image: string;
  description: string;
  markdownPath: string;
  date: string;
  locationDetails: string;
  slug: string;
  tags: Tag[];
}
