import { useFormContext } from "react-hook-form";
import { InputProps } from "./Input";

export const InputText = (props: InputProps) => {
  const { register } = useFormContext();

  return (
    <input
      id={props.id}
      type={props.type}
      className={`rounded-md border-2 p-3 font-medium placeholder:opacity-50 ${
        props.type === "number" && "w-32"
      } `}
      placeholder={props.placeholder}
      {...register(props.name, props.validation)}
    />
  );
};

export default InputText;
