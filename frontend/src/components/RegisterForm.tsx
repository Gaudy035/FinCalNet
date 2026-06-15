import InputTemp from './subcomponents/InputTemp';
import ButtonTemp from './subcomponents/ButtonTemp';
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

export default function RegisterForm() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const formData = new FormData(event.currentTarget);
    const formValues = Object.fromEntries(formData.entries());

    const payload = { ...formValues };

    try {
      await api.post('/register', payload);
      setSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 1500);
    } catch (err: any) {
      setError(err.response?.data?.detail);
      console.log('Blad rejestracji', err);
    }
  };

  return (
    <div className=' min-w-full flex flex-1 justify-center items-center'>
      <form
        className='border-2 flex flex-col justify-center items-center px-12 py-6 gap-6'
        onSubmit={handleSubmit}
      >
        <h1 className='font-bold text-2xl'>REJESTRACJA</h1>
        <InputTemp inpType='text' inpText='Imie:' inpId='imie' inpName='imie' />
        <InputTemp
          inpType='text'
          inpText='Nazwisko:'
          inpId='nazwisko'
          inpName='nazwisko'
        />
        <InputTemp
          inpType='email'
          inpText='Email:'
          inpId='email'
          inpName='email'
        />
        <InputTemp
          inpType='password'
          inpText='Haslo:'
          inpId='haslo'
          inpName='haslo'
        />
        <ButtonTemp
          btnClick={() => console.log('SignIN!!!')}
          btnText='Zarejestruj'
          btnType='submit'
        />
        <div>
          <h2 className='text-red-600'>{error ? error : ''}</h2>
          <h2 className='text-green-600'>
            {success ? 'Rejestracja pomyslna' : ''}
          </h2>
        </div>
      </form>
    </div>
  );
}
