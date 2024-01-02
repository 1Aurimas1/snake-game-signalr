import { DateTimePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { Controller, useFormContext } from "react-hook-form";

interface Props {
  name: string;
  label: string;
}

export const InputDateTime = (props: Props) => {
  const { control } = useFormContext();

  return (
    <Controller
      name={props.name}
      control={control}
      defaultValue={dayjs()}
      render={({ field }) => (
        <LocalizationProvider dateAdapter={AdapterDayjs}>
          <DateTimePicker
            className="rounded-md border-2 p-3 font-medium placeholder:opacity-50"
            value={field.value}
            onChange={(newValue) => {
              field.onChange(newValue);
            }}
          />
        </LocalizationProvider>
      )}
    />
  );
};

export default InputDateTime;
