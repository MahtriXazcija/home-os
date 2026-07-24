import { apiDelete, apiGet, apiPost, ApiError } from "./client";

export interface Member {
  userId: string;
  displayName: string;
  role: "Owner" | "Member";
  joinedAtUtc: string;
}

export interface Household {
  id: string;
  name: string;
  createdAtUtc: string;
  members: Member[];
}

export interface Invitation {
  id: string;
  email: string;
  token: string;
  expiresAtUtc: string;
}

export async function getMyHousehold(): Promise<Household | null> {
  try {
    return await apiGet<Household>("/api/households/mine");
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) return null;
    throw err;
  }
}

export const createHousehold = (name: string) => apiPost<Household>("/api/households", { name });

export const inviteMember = (householdId: string, email: string) =>
  apiPost<Invitation>(`/api/households/${householdId}/invitations`, { email });

export const acceptInvitation = (token: string) =>
  apiPost<Household>("/api/households/invitations/accept", { token });

export const removeMember = (householdId: string, userId: string) =>
  apiDelete(`/api/households/${householdId}/members/${userId}`);
