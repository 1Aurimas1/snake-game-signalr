import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { FormProvider, useForm } from "react-hook-form";
import {
  emailValidation,
  passwordConfirmationValidation,
  passwordValidation,
  usernameValidation,
} from "../utils/inputValidations";
import { ApiErrorResponse } from "../shared/interfaces/ApiResponse";
import Input from "../components/Input";
import Button from "../components/Button";
import { useFetch } from "../hooks/useFetch";

interface RegisterUserDto {
  username: string;
  email: string;
  password: string;
  passwordConfirmation: string;
}

export const Register = () => {
  const navigate = useNavigate();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);

  const methods = useForm<RegisterUserDto>();

  passwordConfirmationValidation.validation.validate = (value: string) =>
    value === methods.getValues("password") || "Passwords don't match";

  const onSubmit = methods.handleSubmit(async (registerUserDto) => {
    const { apiError } = await useFetch(
      "/register",
      { method: "POST" },
      registerUserDto,
    );
    if (apiError) {
      setApiError(apiError);
    } else {
      setApiError(null);
      methods.reset();
      navigate("/login", {
        state: { isRegistrationSuccessful: true },
      });
    }
  });

  return (
    <FormProvider {...methods}>
      <form
        onSubmit={(e) => e.preventDefault()}
        noValidate
        autoComplete="off"
        className="flex flex-col items-center justify-center gap-5"
      >
        <Input {...usernameValidation} apiErrorResponse={apiError} />
        <Input {...emailValidation} apiErrorResponse={apiError} />
        <Input {...passwordValidation} apiErrorResponse={apiError} />
        <Input
          {...passwordConfirmationValidation}
          apiErrorResponse={apiError}
        />
        <Button onClick={onSubmit}>Register</Button>
        <p>
          Already have an account?&nbsp;
          <Link
            to="/login"
            className="font-semibold text-blue-500 hover:underline"
          >
            Log in
          </Link>
        </p>
      </form>
    </FormProvider>
  );
};

export default Register;
