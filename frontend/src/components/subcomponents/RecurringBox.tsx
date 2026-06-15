import NavbarLink from './NavbarLink';
import { useNavigate } from 'react-router';

interface RecurringBoxProps {
  id_t_powtarzalnej: number;
  id_kategorii: number | null;
  kwota: number;
  tytul: string;
  opis?: string | null;
  typ: 'wplyw' | 'wydatek';
  nastepny_termin: string;
  wlasciciel_konta?: string | null;
  konto?: string | null;
  metoda?: string;
  co_ile: string;
  czy_aktywna: boolean | number;
}

export default function RecurringBox({
  id_t_powtarzalnej,
  id_kategorii,
  kwota,
  tytul,
  opis,
  wlasciciel_konta,
  konto,
  metoda,
  typ,
  nastepny_termin,
  co_ile,
  czy_aktywna,
}: RecurringBoxProps) {
  const navigate = useNavigate();

  const durConv = function (co_ile: string) {
    switch (co_ile) {
      case 'P30D':
        return 'miesiac';
      case 'P7D':
        return 'tydzien';
      case 'P1Y':
        return 'rok';
    }
  };

  const handleModify = () => {
    navigate('/modify', {
      state: {
        id_t_powtarzalnej,
        id_kategorii,
        kwota,
        tytul,
        opis,
        wlasciciel_konta,
        konto,
        metoda,
        typ,
        nastepny_termin,
        co_ile,
        czy_aktywna,
      },
    });
  };

  return (
    <div className='flex border-2 px-6 py-4 gap-2 flex-col my-4 w-9/10'>
      <div className='flex flex-row justify-between w-full'>
        <h2>{tytul}</h2>
        <div className='flex flex-col justify-start items-end'>
          <h2>Nastepny termin: {nastepny_termin}</h2>
          <p>Platne co {durConv(co_ile)}</p>
        </div>
      </div>
      <div className='flex items-center justify-between'>
        <h3 className={typ === 'wplyw' ? 'text-green-600' : 'text-red-600'}>
          {typ === 'wydatek' ? '-' : ''}
          {kwota} PLN
        </h3>
        <div className='flex flex-col justify-end items-end'>
          <p>{metoda == 'przelew' ? `Konto: ${konto}` : 'Platnosc gotowka'}</p>
          <p>
            {metoda == 'przelew' ? `Własciciel konta: ${wlasciciel_konta}` : ''}
          </p>
        </div>
      </div>
      <div className='flex flex-col justify-center items-start'>
        <p>{opis ? opis : ''}</p>
        <p className={czy_aktywna ? 'text-green-600' : 'text-red-600'}>
          {czy_aktywna ? 'AKTYWNA' : 'NIEAKTYWNA'}
        </p>
        <NavbarLink linkClick={handleModify} linkText='*Modyfikuj*' />
      </div>
    </div>
  );
}
