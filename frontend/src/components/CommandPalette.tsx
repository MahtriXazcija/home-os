import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useApps } from "../hooks/useApps";
import { search as searchApi, type SearchResult } from "../api/search";

interface CommandPaletteProps {
  open: boolean;
  onClose: () => void;
}

export default function CommandPalette({ open, onClose }: CommandPaletteProps) {
  const navigate = useNavigate();
  const { data: apps, householdId } = useApps();
  const [query, setQuery] = useState("");

  const { data: results } = useQuery({
    queryKey: ["search", householdId, query],
    queryFn: () => searchApi(householdId, query),
    enabled: !!householdId && query.trim().length >= 2,
  });

  useEffect(() => {
    if (!open) setQuery("");
  }, [open]);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    if (open) window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  if (!open) return null;

  const installedApps = (apps ?? []).filter((a) => a.isInstalled);
  const matchingApps = installedApps.filter((a) => a.navLabel.toLowerCase().includes(query.toLowerCase()));

  function go(route: string) {
    navigate(route);
    onClose();
  }

  return (
    <div className="palette-overlay" onClick={onClose}>
      <div className="palette" onClick={(e) => e.stopPropagation()}>
        <input
          autoFocus
          className="palette-input"
          placeholder="Search or jump to…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />

        <div className="palette-section-label">Go to</div>
        <div className="palette-list">
          {matchingApps.map((a) => (
            <button key={a.id} type="button" className="palette-item" onClick={() => go(a.navRoute)}>
              {a.navLabel}
            </button>
          ))}
          {matchingApps.length === 0 && <div className="palette-empty">No matching apps.</div>}
        </div>

        {query.trim().length >= 2 && (
          <>
            <div className="palette-section-label">Results</div>
            <div className="palette-list">
              {(results ?? []).map((r: SearchResult) => (
                <button key={`${r.appId}-${r.id}`} type="button" className="palette-item" onClick={() => go(r.route)}>
                  <span>{r.title}</span>
                  <span className="palette-item-meta">{r.entityType}</span>
                </button>
              ))}
              {(results ?? []).length === 0 && <div className="palette-empty">No results.</div>}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
