import { ErrorOrSuccess } from "../../shared/interfaces";

interface Props {
  errorOrSuccess: ErrorOrSuccess | null;
}

const ErrorOrSuccessMessage: React.FC<Props> = ({ errorOrSuccess }) => {
  return (
    <>
      {errorOrSuccess && (
        <p
          className={`border border-gray-500 px-2 font-semibold ${
            (errorOrSuccess.error && "bg-red-100 text-red-500") ||
            (errorOrSuccess.success && "bg-green-100 text-green-500")
          }`}
        >
          {errorOrSuccess.error}
          {errorOrSuccess.success}
        </p>
      )}
    </>
  );
};

export default ErrorOrSuccessMessage;
