import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import {
  emailValidation,
  passwordConfirmationValidation,
  passwordValidation,
  usernameValidation,
} from "../../utils/inputValidations";
import { useFetch } from "../../hooks/useFetch";
import { LOGIN } from "../../shared/constants/Routes";
import { ApiErrorResponse } from "../../shared/interfaces";
import { Button, CustomLink, Form, Input } from "../../components";

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

  async function onSubmit(registerUserDto: RegisterUserDto) {
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
      navigate(LOGIN, {
        state: { isRegistrationSuccessful: true },
      });
    }
  }

  return (
    <Form methods={methods} onSubmit={onSubmit} errorOrSuccess={null}>
      <Input {...usernameValidation} apiErrorResponse={apiError} />
      <Input {...emailValidation} apiErrorResponse={apiError} />
      <Input {...passwordValidation} apiErrorResponse={apiError} />
      <Input {...passwordConfirmationValidation} apiErrorResponse={apiError} />
      <Button type="submit">Register</Button>
      <p>
        Already have an account?&nbsp;
        <CustomLink to={LOGIN} hasButtonStyle={false}>
          Log in
        </CustomLink>
      </p>
    </Form>
  );
};

export default Register;
