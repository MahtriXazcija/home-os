import { useEffect, useMemo, useRef, type ReactNode } from "react";
import Icon, { type IconName } from "./Icon";

const ICONS: IconName[] = [
  "home", "check-square", "columns", "calendar", "bell", "file-text",
  "dollar-sign", "archive", "utensils", "message-circle", "user", "camera",
];

interface Spot { x: number; y: number; icon: IconName; size: number; }

function generateSpots(count: number): Spot[] {
  const cols = 6;
  const rows = Math.ceil(count / cols);
  let seed = 42;
  const rand = () => { seed = (seed * 9301 + 49297) % 233280; return seed / 233280; };

  const spots: Spot[] = [];
  for (let i = 0; i < count; i++) {
    const col = i % cols;
    const row = Math.floor(i / cols);
    const jitterX = (rand() - 0.5) * 12;
    const jitterY = (rand() - 0.5) * 12;
    spots.push({
      x: (col + 0.5) * (100 / cols) + jitterX,
      y: (row + 0.5) * (100 / rows) + jitterY,
      icon: ICONS[i % ICONS.length],
      size: 22 + Math.round(rand() * 14),
    });
  }
  return spots;
}

/**
 * Login/Register-only decorative background: scattered icons that repel
 * from the cursor, like same-pole magnets. Deliberately not used anywhere
 * else — the site-wide doodle background stays static.
 */
export default function AuthBackground({ children }: { children: ReactNode }) {
  const containerRef = useRef<HTMLDivElement>(null);
  const iconRefs = useRef<(HTMLSpanElement | null)[]>([]);
  const spots = useMemo(() => generateSpots(24), []);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const REPEL_RADIUS = 130;
    const REPEL_STRENGTH = 38;

    function onMove(e: MouseEvent) {
      const rect = container!.getBoundingClientRect();
      const mx = e.clientX - rect.left;
      const my = e.clientY - rect.top;

      iconRefs.current.forEach((el, i) => {
        if (!el) return;
        const spot = spots[i];
        const ix = (spot.x / 100) * rect.width;
        const iy = (spot.y / 100) * rect.height;
        const dx = ix - mx;
        const dy = iy - my;
        const dist = Math.sqrt(dx * dx + dy * dy) || 1;

        if (dist < REPEL_RADIUS) {
          const force = (1 - dist / REPEL_RADIUS) * REPEL_STRENGTH;
          const tx = (dx / dist) * force;
          const ty = (dy / dist) * force;
          el.style.transform = `translate(${tx.toFixed(1)}px, ${ty.toFixed(1)}px)`;
        } else {
          el.style.transform = "translate(0px, 0px)";
        }
      });
    }

    function onLeave() {
      iconRefs.current.forEach((el) => {
        if (el) el.style.transform = "translate(0px, 0px)";
      });
    }

    container.addEventListener("mousemove", onMove);
    container.addEventListener("mouseleave", onLeave);
    return () => {
      container.removeEventListener("mousemove", onMove);
      container.removeEventListener("mouseleave", onLeave);
    };
  }, [spots]);

  return (
    <div ref={containerRef} className="auth-screen auth-screen-interactive">
      <div className="auth-bg">
        {spots.map((s, i) => (
          <span
            key={i}
            ref={(el) => { iconRefs.current[i] = el; }}
            className="auth-bg-icon"
            style={{ left: `${s.x}%`, top: `${s.y}%` }}
          >
            <Icon name={s.icon} size={s.size} />
          </span>
        ))}
      </div>
      {children}
    </div>
  );
}
