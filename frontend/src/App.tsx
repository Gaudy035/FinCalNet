import { Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar';
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';
import Dashboard from './components/Dashboard';
import PaymentForm from './components/PaymentForm';
import Calendar from './components/Calendar';
import Charts from './components/Charts';
import PassChange from './components/UserSettings/PassChange';
import EmailChange from './components/UserSettings/EmailChange';
import RecurringForm from './components/RecurringForm';
// import Wip from './components/Wip';

function App() {
  return (
    <div className='min-h-screen flex flex-col bg-neutral-800 text-white font-mono justify-start  '>
      <Navbar />
      <Routes>
        <Route path='/' element={<Dashboard />} />
        <Route path='/login' element={<LoginForm />} />
        <Route path='/pass_change' element={<PassChange />} />
        <Route path='/email_change' element={<EmailChange />} />
        <Route path='/rejestracja' element={<RegisterForm />} />
        <Route path='/nowy' element={<PaymentForm />} />
        <Route path='/kalendarz' element={<Calendar />} />
        <Route path='/wykresy' element={<Charts />} />
        <Route path='/modify' element={<RecurringForm />} />
      </Routes>
    </div>
  );
}

export default App;
