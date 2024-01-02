import { useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import logo from "../../assets/snake_64_bw.png";
import { useForm } from "react-hook-form";
import {
  passwordValidation,
  usernameValidation,
} from "../../utils/inputValidations";
import { useAuth } from "../../hooks/useAuth";
import { useFetch } from "../../hooks/useFetch";
import { REGISTER } from "../../shared/constants/Routes";
import { Button, CustomLink, Form, Input } from "../../components";
import {
  ApiErrorResponse,
  ErrorOrSuccess,
  SuccessfulLoginDto,
} from "../../shared/interfaces";

interface State {
  isRegistrationSuccessful: boolean;
}

interface LoginUserDto {
  username: string;
  password: string;
}

export const Login = () => {
  const { login } = useAuth();
  const location = useLocation();

  const methods = useForm<LoginUserDto>();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  useEffect(() => {
    if (location.state?.isRegistrationSuccessful as State) {
      setErrorOrSuccess({ success: "Account registered successfully!" });
    } else {
      setErrorOrSuccess(null);
    }
  }, []);

  async function onSubmit(loginUserDto: LoginUserDto) {
    const { apiData, apiError } = await useFetch(
      "/login",
      { method: "POST" },
      loginUserDto,
    );

    if (apiError) {
      setApiError(apiError);
    } else {
      methods.reset();
      login({ ...apiData } as SuccessfulLoginDto);
    }
  }

  return (
    <div className="flex flex-col items-center gap-5">
      <h1 className="mb-24 mt-20 min-w-max text-7xl font-thin">
        <img src={logo} className="float-left mr-1" alt="S" />
        nake game
      </h1>
      <Form
        methods={methods}
        onSubmit={onSubmit}
        errorOrSuccess={errorOrSuccess}
      >
        <Input {...usernameValidation} apiErrorResponse={apiError} />
        <Input {...passwordValidation} apiErrorResponse={apiError} />
        <Button type="submit">Log In</Button>
      </Form>
      <p>
        Don't have an account?&nbsp;
        <CustomLink to={REGISTER} hasButtonStyle={false}>
          Register
        </CustomLink>
      </p>
    </div>
  );
};

export default Login;
