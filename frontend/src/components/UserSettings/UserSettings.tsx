import { useNavigate } from 'react-router';
import ButtonTemp from '../subcomponents/ButtonTemp';

interface UserSettingsProps {
  visibility: boolean;
  change: () => void;
}

export default function UserSettings({
  visibility,
  change,
}: UserSettingsProps) {
  const navigate = useNavigate();

  const logOut = () => {
    change();
    localStorage.removeItem('token');
    navigate('/');
  };

  if (!visibility) {
    return null;
  }

  return (
    <>
      <div
        className='max-h-full min-w-full fixed inset-0 z-20'
        onClick={change}
      ></div>
      <div
        className=' flex flex-col justify-center items-center px-6 py-6 gap-6 border-2 absolute top-full right-0 min-w-max bg-neutral-800 z-30'
        onClick={change}
      >
        <h1 className='font-bold'>KONTO</h1>
        <ButtonTemp
          reverse
          btnType='button'
          btnText='Zmien email'
          btnClick={() => navigate('/email_change')}
        />
        <ButtonTemp
          reverse
          btnType='button'
          btnText='Zmien haslo'
          btnClick={() => navigate('/pass_change')}
        />

        <ButtonTemp
          btnText='LOG-OUT'
          btnType='button'
          btnClick={() => logOut()}
        />
      </div>
    </>
  );
}
