import { apiDelete, apiGet, apiPatch, apiPost } from "./client";

export interface HouseholdDoc {
  id: string;
  householdId: string;
  title: string;
  category: string;
  renewalDateUtc: string | null;
  notes: string | null;
}

export interface Contact {
  id: string;
  name: string;
  phone: string | null;
  email: string | null;
  notes: string | null;
}

export interface ShoppingItem {
  id: string;
  text: string;
  isChecked: boolean;
}

export const getDocuments = (householdId: string) => apiGet<HouseholdDoc[]>(`/api/life-admin/documents?householdId=${householdId}`);
export const createDocument = (input: { householdId: string; title: string; category: string; renewalDateUtc?: string | null; notes?: string }) =>
  apiPost<HouseholdDoc>("/api/life-admin/documents", input);
export const deleteDocument = (id: string) => apiDelete(`/api/life-admin/documents/${id}`);

export const getContacts = (householdId: string) => apiGet<Contact[]>(`/api/life-admin/contacts?householdId=${householdId}`);
export const createContact = (input: { householdId: string; name: string; phone?: string; email?: string; notes?: string }) =>
  apiPost<Contact>("/api/life-admin/contacts", input);
export const deleteContact = (id: string) => apiDelete(`/api/life-admin/contacts/${id}`);

export const getShoppingItems = (householdId: string) => apiGet<ShoppingItem[]>(`/api/life-admin/shopping-items?householdId=${householdId}`);
export const addShoppingItem = (householdId: string, text: string) => apiPost<ShoppingItem>("/api/life-admin/shopping-items", { householdId, text });
export const setShoppingItemChecked = (id: string, isChecked: boolean) => apiPatch<void>(`/api/life-admin/shopping-items/${id}`, { isChecked });
export const deleteShoppingItem = (id: string) => apiDelete(`/api/life-admin/shopping-items/${id}`);
