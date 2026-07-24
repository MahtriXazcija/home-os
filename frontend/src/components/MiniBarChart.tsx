interface MiniBarChartProps {
  values: number[];
  labels: string[];
}

export default function MiniBarChart({ values, labels }: MiniBarChartProps) {
  const max = Math.max(1, ...values);
  return (
    <div className="mini-bars">
      {values.map((v, i) => (
        <div key={i} className="mini-bar-col">
          <div
            className="mini-bar"
            style={{ height: `${Math.max(4, (v / max) * 100)}%` }}
            title={`${labels[i]}: $${v.toFixed(2)}`}
          />
          <span className="mini-bar-label">{labels[i]}</span>
        </div>
      ))}
    </div>
  );
}
