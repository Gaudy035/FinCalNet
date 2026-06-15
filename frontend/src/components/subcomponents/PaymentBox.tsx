interface PaymentBoxProps {
  kwota: number;
  tytul: string;
  opis?: string | null;
  typ: 'wplyw' | 'wydatek';
  data: string;
  wlasciciel_konta?: string | null;
  konto?: string | null;
  metoda?: string;
}

export default function PaymentBox({
  kwota,
  tytul,
  opis,
  wlasciciel_konta,
  konto,
  metoda,
  typ,
  data,
}: PaymentBoxProps) {
  return (
    <div className='flex border-2 px-6 py-4 gap-2 flex-col my-4 w-9/10'>
      <div className='flex flex-row justify-between w-full'>
        <h2>{tytul}</h2>
        <h2>{data}</h2>
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
      <div>
        <p>{opis ? opis : ''}</p>
      </div>
    </div>
  );
}
