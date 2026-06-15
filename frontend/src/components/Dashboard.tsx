import RightPanel from './Dashboard/RightPanel';
import LeftPanel from './Dashboard/LeftPanel';
import ButtonTemp from './subcomponents/ButtonTemp';
import { useNavigate } from 'react-router';

export default function Dashboard() {
  const navigate = useNavigate();
  const token = localStorage.getItem('token');

  return (
    <div className='flex flex-row min-w-full min-h-full flex-1 justify-center  gap-3 p-4 relative'>
      <LeftPanel />
      <RightPanel />
      <div
        className={
          token
            ? 'absolute top-12 left-1/2 -translate-x-1/2 -translate-y-1/2'
            : 'hidden'
        }
      >
        <ButtonTemp
          btnText='DODAJ NOWY'
          btnType='button'
          btnClick={() => navigate('/nowy')}
          reverse
        />
      </div>
    </div>
  );
}
