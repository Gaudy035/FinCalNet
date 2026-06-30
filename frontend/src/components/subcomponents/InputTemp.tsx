interface InputTempProps {
  inpType: string;
  inpText: string;
  inpId: string;
  inpName: string;
  optional?: boolean;
  inpVal?: string | number;
}

export default function InputTemp({
  inpType,
  inpText,
  inpId,
  inpName,
  optional,
  inpVal,
}: InputTempProps) {
  return (
    <input
      type={inpType}
      step={inpType === 'number' ? 0.01 : undefined}
      placeholder={inpText}
      id={inpId}
      name={inpName}
      required={!optional}
      defaultValue={inpVal ? inpVal : ''}
      className='bg-neutral-800 border-2 border-white py-2 px-4 w-full'
    />
  );
}
