type SkeletonBaseProps = {
  className?: string;
};

type BoxProps = SkeletonBaseProps & {
  width?: string | number;
  height?: string | number;
};

const Box = (props: BoxProps) => {
  const style = {
    width: typeof props.width === 'number' ? `${props.width}px` : props.width,
    height: typeof props.height === 'number' ? `${props.height}px` : props.height,
  };

  return (
    <div 
      className={`skeleton ${props.className || ''}`} 
      style={style}
    />
  );
};

type TextProps = SkeletonBaseProps & {
  width?: string | number;
  lines?: number;
};

const Text = (props: TextProps) => {
  const lineCount = props.lines || 1;
  const style = {
    width: typeof props.width === 'number' ? `${props.width}px` : props.width,
    height: '1em',
  };

  if (lineCount === 1) {
    return (
      <div 
        className={`skeleton ${props.className || ''}`} 
        style={style}
      />
    );
  }

  return (
    <div className={props.className}>
      {Array.from({ length: lineCount }).map((_, idx) => (
        <div 
          key={idx}
          className="skeleton mb-2" 
          style={{
            ...style,
            width: idx === lineCount - 1 ? '75%' : style.width,
          }}
        />
      ))}
    </div>
  );
};

type CircleProps = SkeletonBaseProps & {
  size?: string | number;
};

const Circle = (props: CircleProps) => {
  const size = typeof props.size === 'number' ? `${props.size}px` : (props.size || '40px');

  return (
    <div 
      className={`skeleton skeleton-circle ${props.className || ''}`} 
      style={{ width: size, height: size }}
    />
  );
};

type ButtonProps = SkeletonBaseProps & {
  width?: string | number;
};

const Button = (props: ButtonProps) => {
  const style = {
    width: typeof props.width === 'number' ? `${props.width}px` : (props.width || '80px'),
    height: '38px',
  };

  return (
    <div 
      className={`skeleton skeleton-button ${props.className || ''}`} 
      style={style}
    />
  );
};

export const Skeleton = {
  Box,
  Text,
  Circle,
  Button,
};

