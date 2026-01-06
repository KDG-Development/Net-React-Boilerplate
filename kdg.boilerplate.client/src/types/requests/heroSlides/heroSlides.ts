import { THeroSlide } from "../../crm/heroSlide";

export type THeroSlideFilters = {
  isActive?: boolean;
};

export type TCreateHeroSlideDto =
  & Pick<THeroSlide, 'buttonText' | 'buttonUrl'>
  & Partial<Pick<THeroSlide, 'sortOrder' | 'isActive'>>
  & { image: File };

export type TUpdateHeroSlideDto = Partial<Pick<
  THeroSlide,
  | 'buttonText'
  | 'buttonUrl'
  | 'sortOrder'
  | 'isActive'
>>;

export type TReorderHeroSlidesDto = {
  slideIds: string[];
};

