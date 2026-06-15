import { useNavigate } from 'react-router';
import { useState, type SyntheticEvent } from 'react';
import ButtonTemp from '../subcomponents/ButtonTemp';
import InputTemp from '../subcomponents/InputTemp';
import api from '../../api';

export default function PassChange() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const formData = new FormData(event.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await api.put('/update_password', payload);
      setSuccess(true);
      setTimeout(() => {
        localStorage.removeItem('token');
        navigate('/login');
      }, 1500);
    } catch (err: any) {
      setError(err.response?.data?.detail);
      console.log('Blad polaczenia z API', err);
    }
  };

  return (
    <div className='flex flex-1 min-w-full min-h-full justify-center items-center'>
      <form
        onSubmit={handleSubmit}
        className='flex flex-col border-2 py-6 px-12 gap-6 justify-center items-center'
      >
        <h1 className='font-bold text-2xl'>Zmiana hasla</h1>
        <InputTemp
          inpId='new_pass'
          inpName='new_pass'
          inpText='Nowe haslo:'
          inpType='password'
        />
        <InputTemp
          inpId='current_pass'
          inpName='current_pass'
          inpText='Aktualne haslo:'
          inpType='password'
        />
        <ButtonTemp btnText='ZAPISZ' btnType='submit' />
        <p className={success ? 'text-green-600' : 'text-red-600'}>
          {success ? 'Zmiana hasla pomyslna' : ''}
          {error ? error : ''}
        </p>
      </form>
    </div>
  );
}
