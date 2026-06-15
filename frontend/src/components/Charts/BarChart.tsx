import {
  Chart as ChartJS,
  LinearScale,
  CategoryScale,
  BarElement,
  Title,
  plugins,
  Tooltip,
  Legend,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { useState, useEffect } from 'react';
import api from '../../api';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  plugins,
  Tooltip,
  Legend,
);

interface Summary {
  typ: string;
  kwota: number;
}

export default function BarChart() {
  const token = localStorage.getItem('token');
  const [summary, setSummary] = useState<Summary[]>([]);
  const wplywy = summary.find((s) => s.typ === 'wplyw')?.kwota || 0;
  const wydatki = summary.find((s) => s.typ === 'wydatek')?.kwota || 0;
  let bilans: number = wplywy - wydatki;

  useEffect(() => {
    if (!token) {
      setSummary([]);
      return;
    }

    api
      .get('/get_summary')
      .then((response) => setSummary(response.data))
      .catch((error) => console.log('Blad polaczenia z API', error));
  }, [token]);

  const options = {
    plugins: {
      legend: {
        position: 'bottom' as const,
        labels: {
          padding: 20,
          color: 'white' as const,
          font: {
            size: 16,
          },
        },
      },
    },
  };

  const chartData = {
    labels: ['Transakcje'],
    datasets: [
      {
        label: 'Wplywy',
        data: [wplywy],
        backgroundColor: 'rgba(75, 192, 192, 0.6)',
        borderColor: 'rgba(75, 192, 192, 1)',
        borderWidth: 1,
        borderRadius: 8,
      },
      {
        label: 'Wydatki',
        data: [wydatki],
        backgroundColor: 'rgba(255, 99, 132, 0.6)',
        borderColor: 'rgba(255, 99, 132, 1)',
        borderWidth: 1,
        borderRadius: 8,
      },
    ],
  };

  return (
    <div className='flex justify-start flex-col items-center w-full max-h-screen flex-1'>
      <div className='flex flex-1 justify-start flex-col py-6 w-2/3 items-center h-full'>
        <div className='flex justify-center items-center gap-2'>
          <h1>Bilans:</h1>
          <p className={bilans >= 0 ? 'text-green-600' : 'text-red-600'}>
            {bilans >= 0 ? `${bilans}` : `-${bilans}`}
          </p>
        </div>
        <Bar data={chartData} options={options} />
      </div>
    </div>
  );
}
