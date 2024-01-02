function getWidthUtility(className: string | undefined): string | null {
  if (!className) {
    return null;
  }
  const widthUtilityPattern = /w-\d+/;
  const match = className.match(widthUtilityPattern);
  return match ? match[0] : null;
}

const Button = (props: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
  const width = getWidthUtility(props.className) ?? "w-28";
  return (
    <button
      {...props}
      className={`rounded border border-black bg-gray-500 py-2 text-white hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black ${props.className} ${width}`}
    >
      {props.children}
    </button>
  );
};

export default Button;
