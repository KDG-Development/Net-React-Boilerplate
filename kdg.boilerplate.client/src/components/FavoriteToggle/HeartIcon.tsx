type HeartIconProps = {
  filled?: boolean;
  className?: string;
  size?: number;
};

export const HeartIcon = (props: HeartIconProps) => {
  const size = props.size ?? 20;

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      width={size}
      height={size}
      className={props.className}
      fill={props.filled ? "currentColor" : "none"}
      stroke="currentColor"
      strokeWidth={props.filled ? 0 : 2}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
    </svg>
  );
};

