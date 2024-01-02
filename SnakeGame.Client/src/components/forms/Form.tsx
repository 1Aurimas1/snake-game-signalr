import { ReactNode } from "react";
import { FormProvider, UseFormReturn } from "react-hook-form";
import ErrorOrSuccessMessage from "./ErrorOrSuccessMessage";
import { ErrorOrSuccess } from "../../shared/interfaces";

interface Props {
  errorOrSuccess: ErrorOrSuccess | null;
  methods: UseFormReturn<any, any, undefined>;
  onSubmit: (fieldValues: any) => Promise<void>;
  children: ReactNode;
}

const Form = (props: Props) => {
  return (
    <FormProvider {...props.methods}>
      <form
        onSubmit={props.methods.handleSubmit(props.onSubmit)}
        noValidate
        autoComplete="off"
        className="flex flex-col items-center justify-center gap-5"
      >
        <ErrorOrSuccessMessage errorOrSuccess={props.errorOrSuccess} />
        {props.children}
      </form>
    </FormProvider>
  );
};

export default Form;
