import { HeroSlideManager } from './components/HeroSlideManager'

export const HeroBannerSettings = () => {
  return (
    <section>
      <div className="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
        <h4 className="mb-0">Hero Banner Slides</h4>
      </div>
      <p className="text-muted mb-4">
        Manage the hero banner slideshow displayed on the homepage. Add, edit, reorder, or remove slides.
      </p>
      <HeroSlideManager />
    </section>
  )
}

