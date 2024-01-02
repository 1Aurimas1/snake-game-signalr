import { ReactNode } from "react";
import { Link, To } from "react-router-dom";

interface Props {
  to: To;
  hasButtonStyle: boolean;
  children: ReactNode;
}

const CustomLink = (props: Props) => {
  return (
    <Link
      to={props.to}
      className={
        props.hasButtonStyle
          ? "mt-2 rounded border border-black bg-gray-500 p-2 text-white hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black"
          : "font-semibold text-blue-500 hover:underline"
      }
    >
      {props.children}
    </Link>
  );
};

export default CustomLink;
