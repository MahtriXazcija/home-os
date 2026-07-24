import { apiGet, apiPost } from "./client";

export interface App {
  id: string;
  name: string;
  icon: string;
  description: string;
  isCore: boolean;
  permissions: string[];
  publishes: string[];
  subscribes: string[];
  navLabel: string;
  navRoute: string;
  commandPaletteActions: string[];
  searchProvider: boolean;
  isInstalled: boolean;
  grantedPermissions: string[];
}

export const getApps = (householdId: string) => apiGet<App[]>(`/api/apps?householdId=${householdId}`);

export const installApp = (id: string, householdId: string, grantedPermissions: string[]) =>
  apiPost<App>(`/api/apps/${id}/install`, { householdId, grantedPermissions });

export const uninstallApp = (id: string, householdId: string) =>
  apiPost<void>(`/api/apps/${id}/uninstall`, { householdId });
