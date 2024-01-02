import { ErrorComponent } from "../components";

export const NotFound = () => {
  return (
    <ErrorComponent
      title="404 - Page Not Found"
      helperMessage="The page you are looking for does not exist"
    />
  );
};

export default NotFound;
