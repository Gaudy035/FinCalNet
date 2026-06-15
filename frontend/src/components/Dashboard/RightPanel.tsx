import PaymentBox from '../subcomponents/PaymentBox';
import { useState, useEffect } from 'react';
import type Transakcja from './Transakcja';
import api from '../../api';

export default function RightPanel() {
  const token = localStorage.getItem('token');
  const [wydatki, setWydatki] = useState<Transakcja[]>([]);

  useEffect(() => {
    if (!token) {
      setWydatki([]);
      return;
    }

    api
      .get('/wydatki')
      .then((response) => setWydatki(response.data))
      .catch((error) => console.log('Blad przy pobieraniu wydatkow', error));
  }, [token]);

  return (
    <div className='flex flex-1 max-w-1/2 border-2 border-white justify-start items-center flex-col min-h-full py-6 relative'>
      <h1 className='absolute text-lg -top-3.5 left-48 px-2 bg-neutral-800'>
        Wydatki
      </h1>
      {token
        ? wydatki.map((item) => (
            <PaymentBox
              key={item.id_transakcji}
              kwota={item.kwota}
              typ={item.typ}
              tytul={item.tytul}
              data={item.data}
              metoda={item.metoda}
              wlasciciel_konta={item.wlasciciel_konta}
              konto={item.konto}
            />
          ))
        : 'Zaloguj sie by wyswietlic wydatki'}
    </div>
  );
}
