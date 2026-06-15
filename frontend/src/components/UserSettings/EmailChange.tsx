import { useNavigate } from 'react-router';
import { useState, type SyntheticEvent } from 'react';
import InputTemp from '../subcomponents/InputTemp';
import ButtonTemp from '../subcomponents/ButtonTemp';
import api from '../../api';

export default function EmailChange() {
  const navigate = useNavigate();
  const [success, setSucces] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    const formData = new FormData(event.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await api.put('/update_email', payload);
      setSucces(true);
      setTimeout(() => {
        localStorage.removeItem('token');
        navigate('/login');
      }, 1500);
    } catch (err: any) {
      setError(err.response?.data?.detail);
      console.log('Blad polaczenia z API');
    }
  };

  return (
    <div className='flex flex-1 min-w-full min-h-full justify-center items-center'>
      <form
        onSubmit={handleSubmit}
        className='flex flex-col border-2 py-6 px-12 gap-6 justify-center items-center'
      >
        <h1 className='font-bold text-2xl'>Zmiana Email</h1>
        <InputTemp
          inpId='new_email'
          inpName='new_email'
          inpText='Nowy Email:'
          inpType='email'
        />
        <InputTemp
          inpId='current_pass'
          inpName='current_pass'
          inpText='Aktualne haslo:'
          inpType='password'
        />
        <ButtonTemp btnText='ZAPISZ' btnType='submit' />
        <p className={success ? 'text-green-600' : 'text-red-600'}>
          {success ? 'Zmiana adresu pomyslna' : ''}
          {error ? error : ''}
        </p>
      </form>
    </div>
  );
}
