export default function HomeOSLogo({ size = "md" }: { size?: "sm" | "md" | "lg" }) {
  return (
    <span className={`logo-wordmark logo-${size}`}>
      Home<span className="logo-os-badge">OS</span>
    </span>
  );
}
