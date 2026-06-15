interface ButtonTempProps {
  btnClick?: () => void;
  btnText: string;
  btnType: 'button' | 'submit';
  reverse?: boolean;
}

export default function ButtonTemp({
  btnClick,
  btnText,
  btnType,
  reverse,
}: ButtonTempProps) {
  return (
    <button
      type={btnType}
      onClick={btnClick}
      className={
        reverse
          ? 'border-2 border-white bg-neutral-800 text-white px-2 py-1 font-semibold cursor-pointer hover:bg-white hover:text-neutral-800'
          : 'border-2 border-white bg-white text-neutral-800 px-2 py-1 font-semibold cursor-pointer hover:bg-neutral-800 hover:text-white'
      }
    >
      {btnText}
    </button>
  );
}
