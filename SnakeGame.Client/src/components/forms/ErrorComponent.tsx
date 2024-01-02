import { CustomLink } from "..";

interface Props {
  title: string;
  helperMessage: string;
  redirectPath?: string;
  redirectMessage?: string;
}

const ErrorComponent = (props: Props) => {
  return (
    <div className="flex flex-col items-center justify-center gap-5">
      <h1 className="text-4xl font-semibold capitalize">{props.title}</h1>
      <p className="text-gray-500">{props.helperMessage}</p>
      <CustomLink to={props.redirectPath || "/"} hasButtonStyle={false}>
        {props.redirectMessage || "Go to home page"}
      </CustomLink>
    </div>
  );
};

export default ErrorComponent;
