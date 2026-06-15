import { useEffect, useState } from 'react';
import PaymentBox from '../subcomponents/PaymentBox';
import type Transakcja from '../Dashboard/Transakcja';
import api from '../../api';

export default function Past() {
  const token = localStorage.getItem('token');
  const [transakcje, setTransakcje] = useState<Transakcja[]>([]);

  useEffect(() => {
    if (!token) {
      setTransakcje([]);
      return;
    }

    api
      .get('/transakcje')
      .then((response) => setTransakcje(response.data))
      .catch((error) => console.log('Blad polaczenia z API', error));
  }, [token]);

  return (
    <div className='flex flex-col justify-center items-center w-1/2'>
      {transakcje.map((item) => (
        <PaymentBox
          key={item.id_transakcji}
          kwota={item.kwota}
          typ={item.typ}
          tytul={item.tytul}
          data={item.data}
          konto={item.konto}
          metoda={item.metoda}
          wlasciciel_konta={item.wlasciciel_konta}
          opis={item.opis}
        />
      ))}
    </div>
  );
}
