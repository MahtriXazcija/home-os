import { apiGet } from "./client";

export interface SearchResult {
  appId: string;
  entityType: string;
  id: string;
  title: string;
  route: string;
}

export const search = (householdId: string, query: string) =>
  apiGet<SearchResult[]>(`/api/search?householdId=${householdId}&q=${encodeURIComponent(query)}`);
