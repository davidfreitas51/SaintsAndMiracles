export interface NewSaintDto {
  name: string;
  country: string;
  century: number;
  image: string;
  description: string;
  markdownContent: string;
  title?: string | null;
  feastDay?: string | null;
  patronOf?: string | null;
  religiousOrderId?: number | null;
  tagIds: number[];
}
