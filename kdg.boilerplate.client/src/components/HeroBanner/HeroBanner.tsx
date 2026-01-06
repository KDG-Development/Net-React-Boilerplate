import { useEffect, useState, useCallback } from 'react'
import { ActionButton, Conditional, Icon } from 'kdg-react'
import { THeroSlide } from '../../types/crm/heroSlide'
import { getHeroSlides } from '../../api/crm/heroSlides'
import { BackgroundImage } from '../BackgroundImage'

type THeroBannerProps = {
  autoPlayInterval?: number
}

export const HeroBanner = ({ autoPlayInterval = 5000 }: THeroBannerProps) => {
  const [slides, setSlides] = useState<THeroSlide[]>([])
  const [currentIndex, setCurrentIndex] = useState(0)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getHeroSlides({
      filters: { isActive: true },
      success: (data) => {
        setSlides(data)
        setLoading(false)
      },
      errorHandler: () => {
        setLoading(false)
      }
    })
  }, [])

  const goToSlide = useCallback((index: number) => {
    setCurrentIndex(index)
  }, [])

  const goToPrevious = useCallback(() => {
    setCurrentIndex((prev) => (prev === 0 ? slides.length - 1 : prev - 1))
  }, [slides.length])

  const goToNext = useCallback(() => {
    setCurrentIndex((prev) => (prev === slides.length - 1 ? 0 : prev + 1))
  }, [slides.length])

  useEffect(() => {
    if (slides.length <= 1) return

    const interval = setInterval(goToNext, autoPlayInterval)
    return () => clearInterval(interval)
  }, [slides.length, autoPlayInterval, goToNext])

  if (loading) {
    return <div className="hero-banner hero-banner--loading" />
  }

  if (!slides.length) {
    return null
  }

  const currentSlide = slides[currentIndex]

  return (
    <div className="hero-banner">
      <div className="hero-banner__slides">
        {slides.map((slide, index) => (
          <BackgroundImage
            key={slide.id}
            imageUrl={slide.imageUrl}
            className={`hero-banner__slide ${index === currentIndex ? 'hero-banner__slide--active' : ''}`}
          >
            <div className="hero-banner__overlay" />
            <div className="hero-banner__content">
              <ActionButton
                className="hero-banner__button"
                onClick={() => { window.location.href = currentSlide.buttonUrl }}
              >
                {currentSlide.buttonText}
              </ActionButton>
            </div>
          </BackgroundImage>
        ))}
      </div>

      <Conditional
        condition={slides.length > 1}
        onTrue={() => (
          <>
            <ActionButton
              className="hero-banner__arrow hero-banner__arrow--prev"
              onClick={goToPrevious}
              aria-label="Previous slide"
            >
              <Icon icon={(x) => x.cilChevronLeft} />
            </ActionButton>
            <ActionButton
              className="hero-banner__arrow hero-banner__arrow--next"
              onClick={goToNext}
              aria-label="Next slide"
            >
              <Icon icon={(x) => x.cilChevronRight} />
            </ActionButton>

            <div className="hero-banner__dots">
              {slides.map((slide, index) => (
                <ActionButton
                  key={slide.id}
                  className={`hero-banner__dot ${index === currentIndex ? 'hero-banner__dot--active' : ''}`}
                  onClick={() => goToSlide(index)}
                  aria-label={`Go to slide ${index + 1}`}
                />
              ))}
            </div>
          </>
        )}
      />
    </div>
  )
}

