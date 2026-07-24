import { useState, type FormEvent } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { apiPost, ApiError } from "../api/client";
import HomeOSLogo from "../components/HomeOSLogo";

export default function ResetPassword() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email") ?? "";
  const token = searchParams.get("token") ?? "";

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldError(null);

    if (password.length < 6) {
      setFieldError("Password must be at least 6 characters.");
      return;
    }
    if (password !== confirmPassword) {
      setFieldError("Passwords don't match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await apiPost("/api/auth/reset-password", { email, token, newPassword: password });
      navigate("/login", { state: { passwordReset: true } });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not reset your password — the link may have expired.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (!email || !token) {
    return (
      <div className="auth-screen">
        <div className="auth-card">
          <h1><HomeOSLogo size="lg" /></h1>
          <p className="auth-error">This reset link is missing information — request a new one.</p>
          <p className="auth-switch"><Link to="/forgot-password">Request a new link</Link></p>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-screen">
      <form className="auth-card" onSubmit={handleSubmit}>
        <h1>Home OS</h1>
        <p className="dek">Choose a new password for {email}.</p>

        <label>
          New password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} autoFocus />
        </label>
        <label>
          Confirm password
          <input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required minLength={6} />
        </label>

        {fieldError && <span className="field-error">{fieldError}</span>}
        {error && <p className="auth-error">{error}</p>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving…" : "Reset password"}
        </button>

        <p className="auth-switch">
          <Link to="/login">Back to sign in</Link>
        </p>
      </form>
    </div>
  );
}
