import { Tag } from "../../../interfaces/tag";

export interface Prayer {
  id: number;
  title: string;
  description: string;
  image: string;
  markdownPath: string;
  slug: string;
  tags: Tag[];
}
