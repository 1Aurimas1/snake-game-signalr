interface Props {
  className: string;
  onClick?: React.MouseEventHandler<HTMLDivElement> | undefined;
}

const Cell = (props: Props) => {
  return (
    <div
      className={`h-5 w-5 ${props.className}`}
      onClick={props.onClick}
      role={props.onClick && "button"}
    />
  );
};

export default Cell;
