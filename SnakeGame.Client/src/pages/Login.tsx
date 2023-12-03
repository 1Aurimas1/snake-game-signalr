import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { useRef, useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import Input from "../components/Input";
import {
  passwordValidation,
  usernameValidation,
} from "../utils/inputValidations";
import { ApiErrorResponse } from "../shared/interfaces/ApiError";
import logo from "../assets/snake_64_bw.png";
import Button from "../components/Button";
import SuccessfulLoginDto from "../shared/interfaces/SuccessfulLoginDto";
import { useFetch } from "../hooks/useFetch";

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
  //const isLoginSuccessful = useRef(false);

  const onSubmit = async (loginUserDto: LoginUserDto) => {
    const { apiData, apiError } = await useFetch(
      "/login",
      { method: "POST" },
      loginUserDto,
    );
    //await loginUser(loginUserDto);
    //console.log(apiData);

    //const loginDto: SuccessfulLoginDto = await loginUser(loginUserDto);

    //if (isLoginSuccessful.current && loginDto) {
    if (apiError) {
      setApiError(apiError);
    } else {
      //sessionStorage.setItem("username", formInput.username);
      methods.reset();
      login({ ...apiData } as SuccessfulLoginDto);
    }
  };

  return (
    <div className="flex flex-col items-center gap-5">
      <h1 className="mb-32 mt-24 text-7xl font-thin">
        <img src={logo} className="float-left mr-1" alt="S" />
        nake game
      </h1>
      {(location.state?.isRegistrationSuccessful as State) && (
        <p className="rounded-md bg-green-100 px-2 font-semibold text-green-500">
          Account registered successfully!
        </p>
      )}
      <FormProvider {...methods}>
        <form
          onSubmit={methods.handleSubmit(onSubmit)}
          noValidate
          autoComplete="off"
          className="flex flex-col items-center gap-5"
        >
          <Input {...usernameValidation} apiErrorResponse={apiError} />
          <Input {...passwordValidation} apiErrorResponse={apiError} />
          <Button type="submit">Log In</Button>
        </form>
      </FormProvider>
      <p>
        Don't have an account?&nbsp;
        <Link
          to="/register"
          className="font-semibold text-blue-500 hover:underline"
        >
          Register
        </Link>
      </p>
    </div>
  );
};

export default Login;
