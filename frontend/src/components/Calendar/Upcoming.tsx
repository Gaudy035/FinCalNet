import { useState, useEffect } from 'react';
import type TPowtarzalna from '../Dashboard/TPowtarzalna';
import RecurringBox from '../subcomponents/RecurringBox';
import api from '../../api';

export default function Upcoming() {
  const token = localStorage.getItem('token');
  const [transakcje, setTransakcje] = useState<TPowtarzalna[]>([]);

  useEffect(() => {
    if (!token) {
      setTransakcje([]);
      return;
    }

    api
      .get('/get_recurring')
      .then((response) => setTransakcje(response.data))
      .catch((error) => console.log('blad polaczenia z API', error));
  }, [token]);

  return (
    <div className='flex flex-col justify-center items-center w-1/2'>
      {transakcje.map((item) => (
        <RecurringBox
          id_t_powtarzalnej={item.id_t_powtarzalnej}
          id_kategorii={item.id_kategorii}
          key={item.id_t_powtarzalnej}
          kwota={item.kwota}
          tytul={item.tytul}
          opis={item.opis}
          wlasciciel_konta={item.wlasciciel_konta}
          konto={item.konto}
          metoda={item.metoda}
          typ={item.typ}
          nastepny_termin={item.nastepny_termin}
          co_ile={item.co_ile}
          czy_aktywna={item.czy_aktywna}
        />
      ))}
    </div>
  );
}
