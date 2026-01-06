import { useState } from 'react'
import { ActionButton, TextInput, Radio, Row, Col, Conditional, AsyncButton, Enums, Clickable, FileInput } from 'kdg-react'
import { THeroSlide } from '../../../types/crm/heroSlide'
import { createHeroSlide, updateHeroSlide, updateHeroSlideImage } from '../../../api/crm/heroSlides'
import { BackgroundImage } from '../../../components/BackgroundImage'

type TSlideFormProps = {
  slide: THeroSlide | null
  onSuccess: () => void
  onCancel: () => void
}

const defaultFormState = {
  buttonText: '',
  buttonUrl: '',
  isActive: true,
  image: null as File | null,
}

type TFormState = typeof defaultFormState

export const SlideForm = (props: TSlideFormProps) => {
  const isEditing = !!props.slide

  const [form, setForm] = useState<TFormState>(() => {
    if (props.slide) {
      return {
        buttonText: props.slide.buttonText,
        buttonUrl: props.slide.buttonUrl,
        isActive: props.slide.isActive,
        image: null,
      }
    }
    return defaultFormState
  })

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async () => {
    setError(null)
    setLoading(true)

    if (isEditing && props.slide) {
      await updateHeroSlide({
        id: props.slide.id,
        body: {
          buttonText: form.buttonText,
          buttonUrl: form.buttonUrl,
          isActive: form.isActive,
        },
        success: async () => {
          if (form.image) {
            const imageFormData = new FormData()
            imageFormData.append('image', form.image)
            await updateHeroSlideImage({
              id: props.slide!.id,
              body: imageFormData,
              success: () => {
                setLoading(false)
                props.onSuccess()
              },
              errorHandler: () => {
                setError('Failed to update image')
                setLoading(false)
              }
            })
          } else {
            setLoading(false)
            props.onSuccess()
          }
        },
        errorHandler: () => {
          setError('Failed to update slide')
          setLoading(false)
        }
      })
    } else {
      await createHeroSlide({
        body: {
          image: form.image!,
          buttonText: form.buttonText,
          buttonUrl: form.buttonUrl,
          isActive: form.isActive,
        },
        success: () => {
          setLoading(false)
          props.onSuccess()
        },
        errorHandler: () => {
          setError('Failed to create slide')
          setLoading(false)
        }
      })
    }
  }

  return (
    <div className="slide-form">
      <Conditional
        condition={!!error}
        onTrue={() => (
          <div className="alert alert-danger mb-3">{error}</div>
        )}
      />

      <Row className="mb-3">
        <Col md={12}>
          <FileInput
            label={isEditing ? 'Replace Image (optional)' : 'Background Image'}
            config={{
              case: 'Single',
              value: {
                value: form.image,
                onChange: (image) => setForm(prev => ({ ...prev, image })),
              },
            }}
            accept={['image/jpeg', 'image/png', 'image/gif', 'image/webp']}
          />
          <Conditional
            condition={isEditing && !!props.slide?.imageUrl}
            onTrue={() => (
              <div className="mt-2">
                <small className="text-muted">Current image:</small>
                <BackgroundImage
                  imageUrl={props.slide!.imageUrl}
                  width="200px"
                  height="100px"
                  borderRadius="4px"
                  className="mt-2"
                />
              </div>
            )}
          />
        </Col>
      </Row>

      <Row className="mb-3">
        <Col md={6}>
          <TextInput
            label="Button Text"
            value={form.buttonText}
            onChange={(buttonText) => setForm(prev => ({ ...prev, buttonText }))}
            placeholder="e.g., Shop Now"
          />
        </Col>
        <Col md={6}>
          <TextInput
            label="Button URL"
            value={form.buttonUrl}
            onChange={(buttonUrl) => setForm(prev => ({ ...prev, buttonUrl }))}
            placeholder="e.g., /products/featured"
          />
        </Col>
      </Row>

      <div className="mb-3 d-flex align-items-center gap-2">
        <Radio
          name="isActive"
          value={form.isActive}
          onChange={() => setForm(prev => ({ ...prev, isActive: !prev.isActive }))}
        />
        <Clickable onClick={() => setForm(prev => ({ ...prev, isActive: !prev.isActive }))}>
          <label>Active</label>
        </Clickable>
      </div>

      <div className="d-flex gap-2 mt-4">
        <AsyncButton
          loading={loading}
          onClick={handleSubmit}
        >
          {isEditing ? 'Update Slide' : 'Create Slide'}
        </AsyncButton>
        <ActionButton
          color={Enums.Color.Secondary}
          variant="outline"
          onClick={props.onCancel}
          disabled={loading}
        >
          Cancel
        </ActionButton>
      </div>
    </div>
  )
}

