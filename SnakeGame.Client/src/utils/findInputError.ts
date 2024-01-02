import { FieldErrors, FieldValues } from "react-hook-form";

export function getInputErrorsIfExists(
  errors: FieldErrors<FieldValues>,
  name: string,
): string | null {
  for (const key of Object.keys(errors)) {
    if (key.includes(name)) {
      return errors[key]?.message?.toString() ?? null;
    }
  }

  return null;
}
