import ButtonTemp from './subcomponents/ButtonTemp';
import NavbarLink from './subcomponents/NavbarLink';
import { useNavigate } from 'react-router';
import UserSettings from './UserSettings/UserSettings';
import { useState } from 'react';
import Logo from './subcomponents/Logo';

export default function Navbar() {
  const navigate = useNavigate();
  const token = localStorage.getItem('access_token');
  const [vis, setVis] = useState<boolean>(false);

  return (
    <div className='px-4 pt-4 sticky top-0 z-10 bg-neutral-800'>
      <div className='flex flex-row w-full justify-between items-center px-12 h-20 border-2 relative '>
        {/* Logo */}
        <div>
          <h1
            onClick={() => navigate('/')}
            className='font-semibold cursor-pointer'
          >
            <Logo />
          </h1>
        </div>
        {/* Linki */}
        <div className='flex items-center gap-12 justify-center'>
          <NavbarLink linkText='DASHBOARD' linkClick={() => navigate('/')} />
          <NavbarLink
            linkText='KALENDARZ'
            linkClick={() => navigate('/kalendarz')}
          />
          <NavbarLink
            linkText='WYKRESY'
            linkClick={() => navigate('/wykresy')}
          />
        </div>
        {/* Buttony */}
        <div className='flex items-center justify-center '>
          <ButtonTemp
            btnText={token ? 'SETTINGS' : 'LOG-IN'}
            btnClick={
              token
                ? () => (vis ? setVis(false) : setVis(true))
                : () => navigate('/login')
            }
            btnType='button'
          />
          <UserSettings visibility={vis} change={() => setVis(false)} />
        </div>
      </div>
    </div>
  );
}
