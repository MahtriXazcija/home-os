interface CircularGaugeProps {
  percent: number;
  label: string;
  size?: number;
}

export default function CircularGauge({ percent, label, size = 78 }: CircularGaugeProps) {
  const clamped = Math.max(0, Math.min(100, percent));
  const radius = (size - 10) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference * (1 - clamped / 100);

  return (
    <div className="gauge">
      <div className="gauge-ring" style={{ width: size, height: size }}>
        <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
          <circle cx={size / 2} cy={size / 2} r={radius} className="gauge-track" strokeWidth={7} fill="none" />
          <circle
            cx={size / 2}
            cy={size / 2}
            r={radius}
            className="gauge-fill"
            strokeWidth={7}
            fill="none"
            strokeDasharray={circumference}
            strokeDashoffset={offset}
            strokeLinecap="round"
            transform={`rotate(-90 ${size / 2} ${size / 2})`}
          />
        </svg>
        <div className="gauge-percent">{Math.round(clamped)}%</div>
      </div>
      <div className="gauge-label">{label}</div>
    </div>
  );
}
