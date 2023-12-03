export const Button = (
  props: React.ButtonHTMLAttributes<HTMLButtonElement>,
) => {
  return (
    <button
      {...props}
      className={`w-28 rounded bg-gray-500 py-2 text-white hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black ${props.className}`}
    >
      {props.children}
    </button>
  );
};

export default Button;
