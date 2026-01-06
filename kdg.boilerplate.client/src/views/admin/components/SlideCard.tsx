import { Clickable, Badge, Enums, Image, ConfirmModal, ActionButton, Icon } from 'kdg-react'
import { THeroSlide } from '../../../types/crm/heroSlide'

type TSlideCardProps = {
  slide: THeroSlide
  onEdit: () => void
  onDelete: () => void
  onToggleActive: () => void
}

export const SlideCard = (props: TSlideCardProps) => {
  return (
    <div className="border rounded h-100 overflow-hidden bg-white d-flex">
      {/* Drag Handle - Left Side */}
      <div className="d-flex align-items-center justify-content-center px-2 bg-light border-end text-muted" style={{ cursor: 'grab' }}>
        <Icon icon={(x) => x.cilMenu} size="sm" />
      </div>

      {/* Card Content */}
      <div className="flex-grow-1">
        {/* Image Preview */}
        <div className="position-relative">
          <Image
            src={props.slide.imageUrl}
            height={140}
            width="100%"
            className="object-fit-cover"
          />
          {/* Status badge */}
          <Clickable onClick={props.onToggleActive} className="position-absolute top-0 end-0 m-2">
            <Badge color={props.slide.isActive ? Enums.Color.Success : Enums.Color.Secondary}>
              {props.slide.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </Clickable>
        </div>

        {/* Content */}
        <div className="p-3">
          <div className="d-flex align-items-center small mb-3">
            <span className="me-2 fw-medium">{props.slide.buttonText}</span>
            <span className="text-muted text-truncate">→ {props.slide.buttonUrl}</span>
          </div>

          {/* Actions */}
          <div className="d-flex align-items-center gap-2 pt-2 border-top">
            <Clickable onClick={props.onEdit} className="small text-primary fw-medium">
              Edit
            </Clickable>
            <span className="text-muted">·</span>
            <ConfirmModal
              trigger={<span className="small text-danger" role="button">Delete</span>}
              header="Delete Slide"
              content={(cancel) => (
                <div>
                  <p>Are you sure you want to delete this slide?</p>
                  <div className="d-flex gap-2 justify-content-end">
                    <ActionButton color={Enums.Color.Secondary} variant="outline" onClick={cancel}>
                      Cancel
                    </ActionButton>
                    <ActionButton color={Enums.Color.Danger} onClick={() => { props.onDelete(); cancel(); }}>
                      Delete
                    </ActionButton>
                  </div>
                </div>
              )}
            />
          </div>
        </div>
      </div>
    </div>
  )
}

