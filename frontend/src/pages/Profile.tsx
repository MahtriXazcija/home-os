import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getProfile, updateProfile } from "../api/profile";
import { useHousehold } from "../hooks/useHousehold";
import { useAuth } from "../auth/AuthContext";
import { ApiError } from "../api/client";
import Icon from "../components/Icon";

const PHONE_PATTERN = /^\+?[0-9\s\-()]{6,20}$/;
const MAX_PHOTO_DIMENSION = 160;

function resizeToDataUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("Could not read file."));
    reader.onload = () => {
      const img = new Image();
      img.onerror = () => reject(new Error("That doesn't look like a valid image."));
      img.onload = () => {
        const scale = Math.min(1, MAX_PHOTO_DIMENSION / Math.max(img.width, img.height));
        const w = Math.round(img.width * scale);
        const h = Math.round(img.height * scale);
        const canvas = document.createElement("canvas");
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext("2d");
        if (!ctx) return reject(new Error("Canvas not supported."));
        ctx.drawImage(img, 0, 0, w, h);
        resolve(canvas.toDataURL("image/jpeg", 0.82));
      };
      img.src = reader.result as string;
    };
    reader.readAsDataURL(file);
  });
}

export default function Profile() {
  const { user } = useAuth();
  const { data: household } = useHousehold();
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { data: profile, isLoading } = useQuery({ queryKey: ["profile"], queryFn: getProfile });

  const [displayName, setDisplayName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [photoDataUrl, setPhotoDataUrl] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{ displayName?: string; phoneNumber?: string; photo?: string }>({});
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    if (!profile) return;
    setDisplayName(profile.displayName);
    setPhoneNumber(profile.phoneNumber ?? "");
    setPhotoDataUrl(profile.photoDataUrl);
  }, [profile]);

  const saveMutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: (updated) => {
      queryClient.setQueryData(["profile"], updated);
      setSaved(true);
      setTimeout(() => setSaved(false), 2500);
    },
  });

  async function handlePhotoChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;
    if (!file.type.startsWith("image/")) {
      setFieldErrors((prev) => ({ ...prev, photo: "Please choose an image file." }));
      return;
    }
    try {
      const dataUrl = await resizeToDataUrl(file);
      setPhotoDataUrl(dataUrl);
      setFieldErrors((prev) => ({ ...prev, photo: undefined }));
    } catch (err) {
      setFieldErrors((prev) => ({ ...prev, photo: err instanceof Error ? err.message : "Could not process that image." }));
    }
  }

  function validate(): boolean {
    const errors: typeof fieldErrors = {};
    const trimmedName = displayName.trim();
    if (trimmedName.length < 2 || trimmedName.length > 100) {
      errors.displayName = "Name must be between 2 and 100 characters.";
    }
    const trimmedPhone = phoneNumber.trim();
    if (trimmedPhone && !PHONE_PATTERN.test(trimmedPhone)) {
      errors.phoneNumber = "Use digits, spaces, +, -, and parentheses only (6-20 characters).";
    }
    setFieldErrors((prev) => ({ ...prev, ...errors, displayName: errors.displayName, phoneNumber: errors.phoneNumber }));
    return Object.keys(errors).length === 0;
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    saveMutation.reset();
    if (!validate()) return;
    saveMutation.mutate({
      displayName: displayName.trim(),
      phoneNumber: phoneNumber.trim() || null,
      photoDataUrl,
    });
  }

  const myRole = household?.members.find((m) => m.userId === user?.userId)?.role;
  const roleLabel = myRole === "Owner" ? "Administrator" : myRole === "Member" ? "Member" : null;

  if (isLoading) {
    return <p className="dek">Loading profile…</p>;
  }

  return (
    <div>
      <h1>Profile</h1>
      <p className="dek">Your personal details — visible to the members of your household.</p>

      <form className="profile-card" onSubmit={handleSubmit}>
        <div className="profile-photo-row">
          <button
            type="button"
            className="profile-photo-button"
            onClick={() => fileInputRef.current?.click()}
            aria-label="Change photo"
          >
            {photoDataUrl ? (
              <img src={photoDataUrl} alt="" className="profile-photo" />
            ) : (
              <span className="profile-photo-placeholder"><Icon name="user" size={26} /></span>
            )}
            <span className="profile-photo-edit"><Icon name="camera" size={13} /></span>
          </button>
          <input ref={fileInputRef} type="file" accept="image/*" onChange={handlePhotoChange} hidden />
          <div className="profile-photo-meta">
            <div className="profile-name-display">{profile?.displayName}</div>
            {roleLabel && (
              <span className="pill role-pill">
                <Icon name="shield" size={11} />
                {roleLabel}
              </span>
            )}
            {fieldErrors.photo && <p className="auth-error">{fieldErrors.photo}</p>}
          </div>
        </div>

        <label>
          Display name
          <input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />
          {fieldErrors.displayName && <span className="field-error">{fieldErrors.displayName}</span>}
        </label>

        <label>
          Email
          <input value={profile?.email ?? ""} disabled />
        </label>

        <label>
          Phone number <span className="field-optional">(optional)</span>
          <input
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            placeholder="+1 555 123 4567"
          />
          {fieldErrors.phoneNumber && <span className="field-error">{fieldErrors.phoneNumber}</span>}
        </label>

        {saveMutation.isError && (
          <p className="auth-error">
            {saveMutation.error instanceof ApiError ? saveMutation.error.message : "Could not save your profile."}
          </p>
        )}
        {saved && <p className="profile-saved">Saved.</p>}

        <button type="submit" disabled={saveMutation.isPending}>
          {saveMutation.isPending ? "Saving…" : "Save changes"}
        </button>
      </form>
    </div>
  );
}
