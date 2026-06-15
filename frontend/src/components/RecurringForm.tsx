import { useNavigate, useLocation } from 'react-router';
import InputTemp from './subcomponents/InputTemp';
import ButtonTemp from './subcomponents/ButtonTemp';
import { useState, useEffect, type SyntheticEvent } from 'react';
import api from '../api';

interface Kategoria {
  id_kategorii: number;
  nazwa: string;
}

export default function RecurringForm() {
  const navigate = useNavigate();
  const location = useLocation();
  const editData = location.state;
  const [kategorie, setKategorie] = useState<Kategoria[]>([]);

  useEffect(() => {
    api
      .get('/kategorie')
      .then((response) => setKategorie(response.data))
      .catch((error) => console.log('Blad przy pobieraniu kategorii: ', error));
  }, []);

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const formValues = Object.fromEntries(formData.entries());
    const payload = {
      ...formValues,
      czy_aktywna: formData.get('czy_aktywna') === 'on',
    };

    try {
      await api.put(`/modify_recurring/${editData.id_t_powtarzalnej}`, payload);
      navigate('/kalendarz');
    } catch (error) {
      console.log('Blad polaczenia z API', error);
    }
  };

  return (
    <div className='flex flex-1 justify-center items-center min-w-full'>
      <form
        className='border-2 flex flex-col justify-center items-center px-12 py-6 gap-6'
        onSubmit={handleSubmit}
      >
        <h1>MODYFIKUJ TRANSAKCJE</h1>
        <div className='flex flex-row justify-center items-start gap-6'>
          {/* Lewa strona */}
          <div className='flex flex-col justify-center items-center gap-6 min-h-full'>
            <InputTemp
              inpId='tytul'
              inpText='Tytul:'
              inpName='tytul'
              inpType='text'
              inpVal={editData?.tytul}
            />

            <textarea
              name='opis'
              id='opis'
              placeholder='Opis:'
              required
              className='bg-neutral-800 border-2 border-white py-2 px-4 resize-none'
              defaultValue={editData?.opis}
            ></textarea>

            <InputTemp
              inpId='konto'
              inpText='Konto:'
              inpName='konto'
              inpType='number'
              inpVal={editData?.konto}
              optional
            />

            <InputTemp
              inpId='wlasciciel_konta'
              inpText='Wlasciciel Konta:'
              inpName='wlasciciel_konta'
              inpType='text'
              inpVal={editData?.wlasciciel_konta}
              optional
            />
          </div>

          {/* Prawa strona */}
          <div className='flex flex-col justify-center items-center gap-6'>
            <InputTemp
              inpId='nastepny_termin'
              inpText='Nastepny termin:'
              inpName='nastepny_termin'
              inpType='date'
              inpVal={editData?.nastepny_termin}
            />

            <InputTemp
              inpId='kwota'
              inpText='Kwota:'
              inpName='kwota'
              inpType='number'
              inpVal={editData?.kwota}
            />

            <div className='flex justify-between items-center min-w-full px-2'>
              <div className='flex gap-2 justify-center items-center'>
                <input
                  type='radio'
                  name='typ'
                  id='wydatek'
                  value='wydatek'
                  defaultChecked={editData?.typ === 'wydatek'}
                />
                <label htmlFor='wydatek'>Wydatek</label>
              </div>
              <div className='flex gap-2 justify-center items-center'>
                <input
                  type='radio'
                  name='typ'
                  id='wplyw'
                  value='wplyw'
                  defaultChecked={editData?.typ === 'wplyw'}
                />
                <label htmlFor='wplyw'>Wplyw</label>
              </div>
            </div>

            <div className='flex gap-2'>
              <p>Kategoria:</p>
              {kategorie.length > 0 ? (
                <select
                  defaultValue={editData?.id_kategorii}
                  name='id_kategorii'
                  id='id_kategorii'
                  className='border-2'
                  required
                >
                  <option value='' disabled>
                    ---
                  </option>
                  {kategorie.map((item) => (
                    <option key={item.id_kategorii} value={item.id_kategorii}>
                      {item.nazwa}
                    </option>
                  ))}
                </select>
              ) : (
                'Ładownie...'
              )}
            </div>

            <div className='flex justify-between items-center min-w-full px-2'>
              <div className='flex gap-2 justify-center items-center'>
                <input
                  defaultChecked={editData?.metoda === 'przelew'}
                  type='radio'
                  name='metoda'
                  id='przelew'
                  value='przelew'
                />
                <label htmlFor='przelew'>Przelew</label>
              </div>
              <div className='flex gap-2 justify-center items-center'>
                <input
                  defaultChecked={editData?.metoda === 'gotowka'}
                  type='radio'
                  name='metoda'
                  id='gotowka'
                  value='gotowka'
                />
                <label htmlFor='gotowka'>Gotowka</label>
              </div>
            </div>
          </div>
        </div>
        <div className='flex justify-center flex-col gap-2 items-center'>
          <div className='flex justify-center items-center gap-2'>
            <input
              type='checkbox'
              name='czy_aktywna'
              defaultChecked={editData?.czy_aktywna === true}
            />
            <label htmlFor='czy_aktywna'>Czy aktywna?</label>
          </div>
          <select
            name='co_ile'
            id='co_ile'
            className='border-2'
            defaultValue={editData?.co_ile}
          >
            <option value='' disabled>
              Co ile?
            </option>
            <option value='P7D'>Tydzien</option>
            <option value='P30D'>Miesiac</option>
            <option value='P1Y'>Rok</option>
          </select>
        </div>
        <ButtonTemp btnText='ZAPISZ' btnType='submit' />
      </form>
    </div>
  );
}
