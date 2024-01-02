import { FieldValues, RegisterOptions, useFormContext } from "react-hook-form";
import { ReactNode, useEffect } from "react";
import InputError from "./InputError";
import InputDateTime from "./InputDateTime";
import InputText from "./InputText";
import { ApiError, ApiErrorResponse } from "../../shared/interfaces";
import { getInputErrorsIfExists } from "../../utils/findInputError";

export interface InputProps {
  name: string;
  label: string;
  type: string;
  id: string;
  placeholder?: string;
  validation?: RegisterOptions<FieldValues, string>;
  apiErrorResponse: ApiErrorResponse | null;
  children?: ReactNode;
}

function isInputError(inputName: string, err: ApiError): boolean {
  const inputNameLower = inputName.toLowerCase();
  const propertyLower = err.propertyName.toLowerCase();
  const messageLower = err.errorMessage.toLowerCase();

  if (
    propertyLower.includes(inputNameLower) ||
    messageLower.includes(inputNameLower)
  ) {
    return true;
  }
  return false;
}

function renderInputByType(props: InputProps): React.ReactNode {
  switch (props.type) {
    case "date":
      return <InputDateTime {...props} />;
    default:
      return <InputText {...props} />;
  }
}

export const Input = (props: InputProps) => {
  const {
    setError,
    formState: { errors },
  } = useFormContext();

  const inputErrors = getInputErrorsIfExists(errors, props.name);

  useEffect(() => {
    if (props.apiErrorResponse) {
      let errorMessage = "";

      if (Array.isArray(props.apiErrorResponse)) {
        for (const err of props.apiErrorResponse) {
          if (isInputError(props.name, err)) {
            errorMessage += `${err.errorMessage}\n`;
          }
        }
      } else {
        if (isInputError(props.name, props.apiErrorResponse)) {
          errorMessage = props.apiErrorResponse.errorMessage;
        }
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
      {props.children ? props.children : renderInputByType(props)}
    </div>
  );
};

export default Input;
