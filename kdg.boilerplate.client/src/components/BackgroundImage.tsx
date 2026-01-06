import { ReactNode } from 'react'

type TBackgroundImageProps = {
  imageUrl: string
  width?: string
  height?: string
  borderRadius?: string
  className?: string
  children?: ReactNode
}

export const BackgroundImage = (props: TBackgroundImageProps) => (
  <div
    className={props.className}
    style={{
      width: props.width ?? '100%',
      height: props.height ?? '100%',
      backgroundImage: `url(${encodeURI(props.imageUrl)})`,
      backgroundSize: 'cover',
      backgroundPosition: 'center',
      borderRadius: props.borderRadius,
    }}
  >
    {props.children}
  </div>
)

