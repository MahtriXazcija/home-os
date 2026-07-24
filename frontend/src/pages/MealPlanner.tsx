import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { createMealPlanEntry, deleteMealPlanEntry, getMealPlan, type MealType } from "../api/mealPlanner";

const MEAL_TYPES: MealType[] = ["Breakfast", "Lunch", "Dinner"];

function toDateOnly(d: Date) {
  return d.toISOString().slice(0, 10);
}
function startOfWeek(d: Date) {
  const day = d.getDay();
  const result = new Date(d);
  result.setDate(d.getDate() - day);
  return result;
}

export default function MealPlanner() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const [cursor, setCursor] = useState(() => new Date());
  const weekStart = startOfWeek(cursor);
  const days = useMemo(() => Array.from({ length: 7 }, (_, i) => {
    const d = new Date(weekStart);
    d.setDate(weekStart.getDate() + i);
    return d;
  }), [weekStart]);

  const fromDate = toDateOnly(days[0]);
  const toDate = toDateOnly(days[6]);

  const { data: entries } = useQuery({
    queryKey: ["meal-plan", householdId, fromDate, toDate],
    queryFn: () => getMealPlan(householdId, fromDate, toDate),
    enabled: !!householdId,
  });

  const [mealDate, setMealDate] = useState(toDateOnly(new Date()));
  const [mealType, setMealType] = useState<MealType>("Dinner");
  const [title, setTitle] = useState("");
  const [addShoppingTask, setAddShoppingTask] = useState(true);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["meal-plan", householdId] });

  const createMutation = useMutation({
    mutationFn: createMealPlanEntry,
    onSuccess: () => {
      setTitle("");
      invalidate();
      if (addShoppingTask) {
        queryClient.invalidateQueries({ queryKey: ["tasks", householdId] });
      }
    },
  });
  const deleteMutation = useMutation({ mutationFn: deleteMealPlanEntry, onSuccess: invalidate });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!title.trim() || !householdId) return;
    createMutation.mutate({ householdId, mealDate, mealType, title, addShoppingTask });
  }

  const entryFor = (day: Date, type: MealType) =>
    (entries ?? []).find((e) => e.mealDate === toDateOnly(day) && e.mealType === type);

  return (
    <div>
      <h1>Meal Planner</h1>
      <p className="dek">
        A third-party-style app on the platform — plan a meal and optionally turn it into a shopping task
        using the existing Tasks capability, not a task system of its own.
      </p>

      <form className="task-form" onSubmit={handleCreate}>
        <input type="date" value={mealDate} onChange={(e) => setMealDate(e.target.value)} />
        <select value={mealType} onChange={(e) => setMealType(e.target.value as MealType)}>
          {MEAL_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <input className="task-form-title" placeholder="Meal (e.g. Chicken stir-fry)" value={title} onChange={(e) => setTitle(e.target.value)} />
        <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 13 }}>
          <input type="checkbox" checked={addShoppingTask} onChange={(e) => setAddShoppingTask(e.target.checked)} />
          Add shopping task
        </label>
        <button type="submit" disabled={createMutation.isPending}>Add</button>
      </form>

      <div className="calendar-toolbar">
        <button type="button" className="link-button" onClick={() => setCursor(new Date(cursor.getTime() - 7 * 24 * 60 * 60 * 1000))}>← Prev week</button>
        <span className="calendar-month-label">{days[0].toLocaleDateString(undefined, { month: "short", day: "numeric" })} – {days[6].toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
        <button type="button" className="link-button" onClick={() => setCursor(new Date(cursor.getTime() + 7 * 24 * 60 * 60 * 1000))}>Next week →</button>
      </div>

      <div className="meal-grid">
        <div className="meal-grid-corner" />
        {days.map((d) => (
          <div key={d.toISOString()} className="meal-grid-day">{d.toLocaleDateString(undefined, { weekday: "short", day: "numeric" })}</div>
        ))}
        {MEAL_TYPES.map((type) => (
          <>
            <div key={type} className="meal-grid-type">{type}</div>
            {days.map((d) => {
              const entry = entryFor(d, type);
              return (
                <div key={`${type}-${d.toISOString()}`} className="meal-grid-cell">
                  {entry && (
                    <div className="meal-grid-entry">
                      <span>{entry.title}</span>
                      <button type="button" className="link-button" onClick={() => deleteMutation.mutate(entry.id)}>×</button>
                    </div>
                  )}
                </div>
              );
            })}
          </>
        ))}
      </div>
    </div>
  );
}
