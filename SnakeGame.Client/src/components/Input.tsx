import { FieldValues, RegisterOptions, useFormContext } from "react-hook-form";
import { getInputErrorMessage } from "../utils/findInputError";
import { ApiErrorResponse } from "../shared/interfaces/ApiResponse";
import { useEffect } from "react";

interface Props {
  name: string;
  label: string;
  type: string;
  id: string;
  placeholder: string;
  validation?: RegisterOptions<FieldValues, string>;
  apiErrorResponse: ApiErrorResponse | null;
}

const InputError = ({ message }: { message: string }) => {
  return (
    <p className="whitespace-pre-wrap rounded-md bg-red-100 px-2 font-semibold text-red-500">
      {message}
    </p>
  );
};

export const Input = (props: Props) => {
  const {
    register,
    setError,
    formState: { errors },
  } = useFormContext();

  const inputErrors = getInputErrorMessage(errors, props.name);

  useEffect(() => {
    if (props.apiErrorResponse) {
      let errorMessage = "";

      if (Array.isArray(props.apiErrorResponse)) {
        for (const err of props.apiErrorResponse) {
          const fieldLower = err.propertyName.toLowerCase();
          const messageLower = err.errorMessage.toLowerCase();

          if (
            fieldLower.includes(props.name) ||
            messageLower.includes(props.name)
          ) {
            errorMessage += `${err.errorMessage}\n`;
          }
        }
      } else {
        errorMessage = props.apiErrorResponse.errorMessage;
      }

      if (errorMessage) {
        setError(props.name, {
          type: "server",
          message: errorMessage,
        });
      }
    }
  }, [props.apiErrorResponse, setError]);

  return (
    <div className="flex w-72 flex-col gap-2">
      <div>
        <label htmlFor={props.id} className="font-semibold capitalize">
          {props.label}
        </label>
        {inputErrors && <InputError message={inputErrors} key={inputErrors} />}
      </div>
      <input
        id={props.id}
        type={props.type}
        className="rounded-md border-2 p-3 font-medium placeholder:opacity-50"
        placeholder={props.placeholder}
        {...register(props.name, props.validation)}
      />
    </div>
  );
};

export default Input;
