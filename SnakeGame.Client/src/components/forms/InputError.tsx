export const InputError = ({ message }: { message: string }) => {
  return (
    <p className="whitespace-pre-wrap rounded-md bg-red-100 px-2 font-semibold text-red-600">
      {message}
    </p>
  );
};

export default InputError;
