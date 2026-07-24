import { apiDelete, apiGet, apiPost } from "./client";

export type MealType = "Breakfast" | "Lunch" | "Dinner";

export interface MealPlanEntry {
  id: string;
  householdId: string;
  mealDate: string;
  mealType: MealType;
  title: string;
}

export const getMealPlan = (householdId: string, fromDate: string, toDate: string) =>
  apiGet<MealPlanEntry[]>(`/api/meal-planner?householdId=${householdId}&fromDate=${fromDate}&toDate=${toDate}`);

export const createMealPlanEntry = (input: {
  householdId: string;
  mealDate: string;
  mealType: MealType;
  title: string;
  addShoppingTask: boolean;
}) => apiPost<MealPlanEntry>("/api/meal-planner", input);

export const deleteMealPlanEntry = (id: string) => apiDelete(`/api/meal-planner/${id}`);
