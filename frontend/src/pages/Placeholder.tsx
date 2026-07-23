interface PlaceholderProps {
  title: string;
  phase: string;
}

export default function Placeholder({ title, phase }: PlaceholderProps) {
  return (
    <div>
      <h1>{title}</h1>
      <p className="dek">This module ships in {phase} — see the build plan for details.</p>
    </div>
  );
}
