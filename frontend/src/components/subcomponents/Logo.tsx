export default function Logo({ strokeThickness = 2, color = 'currentColor' }) {
  return (
    <div className='flex justify-center items-center gap-2 -mr-14'>
      <svg width='50' viewBox='36.5 44.1 35.7 41.1'>
        <g>
          <rect
            style={{
              fill: 'none',
              stroke: color,
              strokeWidth: strokeThickness,
              strokeLinecap: 'round',
              strokeLinejoin: 'round',
            }}
            x='40.74'
            y='53.28'
            width='27.33'
            height='27.33'
          />
          {/* Trzy pionowe kreski */}
          <path
            style={{
              fill: 'none',
              stroke: color,
              strokeWidth: strokeThickness,
              strokeLinecap: 'round',
              strokeLinejoin: 'round',
            }}
            d='m 45.85,48.43 v 8.04'
          />
          <path
            style={{
              fill: 'none',
              stroke: color,
              strokeWidth: strokeThickness,
              strokeLinecap: 'round',
              strokeLinejoin: 'round',
            }}
            d='m 62.98,48.43 v 8.04'
          />
          <path
            style={{
              fill: 'none',
              stroke: color,
              strokeWidth: strokeThickness,
              strokeLinecap: 'round',
              strokeLinejoin: 'round',
            }}
            d='m 54.42,48.43 v 8.04'
          />
          {/* Znak dolara */}
          <text
            x='44.16'
            y='82.86'
            transform='scale(1.096, 0.912)'
            style={{
              fontSize: '19.78px',
              fontFamily: 'sans-serif',
              fill: 'none',
              stroke: color,
              strokeWidth: '1.5', // Grubość samego dolara
              strokeLinecap: 'round',
              strokeLinejoin: 'round',
            }}
          >
            $
          </text>
        </g>
      </svg>
      <h1 className='text-lg'>Fin_Cal</h1>
    </div>
  );
}
