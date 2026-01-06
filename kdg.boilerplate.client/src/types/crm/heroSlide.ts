export type THeroSlide = {
  id: string;
  imageUrl: string;
  buttonText: string;
  buttonUrl: string;
  sortOrder: number;
  isActive: boolean;
};

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

