import { Link, useNavigate } from "react-router-dom";
import { FormProvider, useForm } from "react-hook-form";
import {
  emailValidation,
  passwordConfirmationValidation,
  passwordValidation,
  usernameValidation,
} from "../utils/inputValidations";
import Input from "./Input";
import { useRef, useState } from "react";
import { ServerError } from "../shared/interfaces/ServerError";

interface FormInput {
  username: string;
  email: string;
  password: string;
  passwordConfirmation: string;
}

interface UserDto {
  username: string;
  email: string;
  password: string;
}

export const Register = () => {
  const [serverErrors, setServerErrors] = useState<ServerError[]>([]);
  const isRegistrationSuccessful = useRef(false);
  const navigate = useNavigate();

  async function registerUser(credentials: UserDto) {
    return await fetch("/api/auth/register", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(credentials),
    })
      .then(async (res) => {
        if (!res.ok) {
          const err = await res.json();

          const errs: ServerError[] = [];
          for (const field in err) {
            const messages = err[field];

            for (const message of messages) {
              errs.push({ field, message });
              break; // only single message per field is displayed
            }
          }
          setServerErrors(errs);

          return await Promise.reject(err);
        }

        setServerErrors([]);
        isRegistrationSuccessful.current = true;

        return res.text();
      })
      .then((data) => {
        return data;
      })
      .catch((e) => {
        console.error("User register fetch error: ", e);
        return null;
      });
  }

  const methods = useForm<FormInput>();

  passwordConfirmationValidation.validation.validate = (value: string) =>
    value === methods.getValues("password") || "Passwords don't match";

  const onSubmit = methods.handleSubmit(async (formInput) => {
    const response = await registerUser(formInput);
    if (isRegistrationSuccessful.current && response) {
      methods.reset();
      navigate("/login", {
        state: { isRegistrationSuccessful: isRegistrationSuccessful },
      });
    }
  });

  return (
    <FormProvider {...methods}>
      <form
        onSubmit={(e) => e.preventDefault()}
        noValidate
        autoComplete="off"
        className="flex h-screen flex-col items-center justify-center gap-5"
      >
        <Input {...usernameValidation} serverError={serverErrors} />
        <Input {...emailValidation} serverError={serverErrors} />
        <Input {...passwordValidation} serverError={serverErrors} />
        <Input {...passwordConfirmationValidation} serverError={serverErrors} />
        <button
          onClick={onSubmit}
          className="w-28 rounded bg-gray-500 py-2 font-bold text-white hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black"
        >
          Register
        </button>
        <p>
          Have an account?&nbsp;
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
