import { useState, useEffect } from 'react'
import { ActionButton, Conditional, Modal, Icon, Loader, Sortable, SortThreshold } from 'kdg-react'
import { THeroSlide } from '../../../types/crm/heroSlide'
import { getHeroSlides, deleteHeroSlide, reorderHeroSlides, updateHeroSlide } from '../../../api/crm/heroSlides'
import { SlideForm } from './SlideForm'
import { SlideCard } from './SlideCard'

export const HeroSlideManager = () => {
  const [slides, setSlides] = useState<THeroSlide[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editingSlide, setEditingSlide] = useState<THeroSlide | null>(null)

  const loadSlides = () => {
    setLoading(true)
    getHeroSlides({
      success: (data) => {
        setSlides(data)
        setLoading(false)
      },
      errorHandler: () => {
        setLoading(false)
      }
    })
  }

  useEffect(() => {
    loadSlides()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleCreateNew = () => {
    setEditingSlide(null)
    setShowForm(true)
  }

  const handleEdit = (slide: THeroSlide) => {
    setEditingSlide(slide)
    setShowForm(true)
  }

  const handleFormClose = () => {
    setShowForm(false)
    setEditingSlide(null)
  }

  const handleFormSuccess = () => {
    handleFormClose()
    loadSlides()
  }

  const handleDelete = (slide: THeroSlide) => {
    deleteHeroSlide({
      id: slide.id,
      success: () => {
        loadSlides()
      },
    })
  }

  const handleResort = (newSlides: THeroSlide[]) => {
    setSlides(newSlides)
    reorderHeroSlides({
      body: { slideIds: newSlides.map(s => s.id) },
      success: () => {},
      errorHandler: () => {
        loadSlides()
      }
    })
  }

  const findSlide = (slide: THeroSlide) => slides.find(s => s.id === slide.id)!

  const handleToggleActive = (slide: THeroSlide) => {
    updateHeroSlide({
      id: slide.id,
      body: { isActive: !slide.isActive },
      success: () => {
        loadSlides()
      },
    })
  }

  return (
    <div className="hero-slide-manager">
      <Conditional
        condition={showForm}
        onTrue={() => (
          <Modal
            onClose={handleFormClose}
            header={() => <h5 className="mb-0">{editingSlide ? 'Edit Slide' : 'Create New Slide'}</h5>}
            content={() => (
              <SlideForm
                slide={editingSlide}
                onSuccess={handleFormSuccess}
                onCancel={handleFormClose}
              />
            )}
            size="lg"
          />
        )}
      />

      <div className="d-flex justify-content-end mb-3">
        <ActionButton onClick={handleCreateNew}>
          Add New Slide
        </ActionButton>
      </div>

      <Conditional
        condition={loading}
        onTrue={() => <Loader />}
        onFalse={() => (
          <Conditional
            condition={!slides.length}
            onTrue={() => (
              <div className="text-center py-5 bg-light rounded">
                <div className="text-muted mb-3">
                  <Icon icon={(x) => x.cilImage} size="3xl" />
                </div>
                <h5 className="text-muted">No slides yet</h5>
                <p className="text-muted small mb-0">Click "Add New Slide" to create your first hero banner.</p>
              </div>
            )}
            onFalse={() => (
              <Sortable
                type="hero-slides"
                items={slides}
                parseKey={(slide) => slide.id}
                sort={SortThreshold.Vertical}
                onResort={handleResort}
                renderItem={{
                  render: (slide) => (
                    <SlideCard
                      slide={findSlide(slide)}
                      onEdit={() => handleEdit(slide)}
                      onDelete={() => handleDelete(slide)}
                      onToggleActive={() => handleToggleActive(slide)}
                    />
                  ),
                  className: 'mb-3'
                }}
              />
            )}
          />
        )}
      />
    </div>
  )
}

