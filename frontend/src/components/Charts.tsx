import { useState } from 'react';
import PieChart from './Charts/PieChart';
import NavbarLink from './subcomponents/NavbarLink';
import BarChart from './Charts/BarChart';

export default function Charts() {
  const token = localStorage.getItem('token');

  const [view, setView] = useState<'pie' | 'bar'>('bar');

  let render = () => {
    if (view === 'pie') {
      return <PieChart />;
    } else {
      return <BarChart />;
    }
  };

  if (!token) {
    return (
      <div className='flex flex-1 flex-col items-center py-4 justify-start w-screen'>
        Zaloguj sie by wyswietlic wykresy
      </div>
    );
  } else {
    return (
      <div className='flex flex-1 flex-col justify-start items-center w-full h-full'>
        <div className='flex justify-center gap-12 py-4'>
          <NavbarLink
            linkClick={() => setView('bar')}
            linkText='Wydatki/wplywy'
          />
          <NavbarLink
            linkClick={() => setView('pie')}
            linkText='Kategorie wydatkow'
          />
        </div>
        {render()}
      </div>
    );
  }
}
