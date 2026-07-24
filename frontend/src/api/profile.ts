import { apiGet, apiPut } from "./client";

export interface Profile {
  userId: string;
  email: string;
  displayName: string;
  phoneNumber: string | null;
  photoDataUrl: string | null;
}

export interface UpdateProfileInput {
  displayName: string;
  phoneNumber: string | null;
  photoDataUrl: string | null;
}

export const getProfile = () => apiGet<Profile>("/api/profile");
export const updateProfile = (input: UpdateProfileInput) => apiPut<Profile>("/api/profile", input);
