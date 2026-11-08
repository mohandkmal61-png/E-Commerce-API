import { useState, type FormEvent } from "react";
import { useAuth } from "../contexts/AuthContext";
import { ApiError } from "../services/apiClient";

type AuthMode = "login" | "register";

function getReturnUrl(): string {
  const requestedPath = new URLSearchParams(window.location.search).get("returnUrl");
  return requestedPath?.startsWith("/") && !requestedPath.startsWith("//")
    ? requestedPath
    : "/basket";
}

export function AuthPage() {
  const { user, login, register, logout } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [username, setUsername] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      if (mode === "login") {
        await login({ email, password });
      } else {
        await register({ email, username, password, displayName, phoneNumber });
      }
      window.location.assign(getReturnUrl());
    } catch (requestError: unknown) {
      setError(
        requestError instanceof ApiError
          ? requestError.message
          : requestError instanceof Error
            ? requestError.message
            : "Authentication failed. Please try again.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="auth-page page-shell" aria-labelledby="auth-heading">
      <a className="back-link" href="/#shop">Continue shopping</a>
      <div className="auth-card">
        <span className="eyebrow">{mode === "login" ? "Welcome back" : "Join Forma"}</span>
        <h1 id="auth-heading">{mode === "login" ? "Sign in to your account" : "Create your account"}</h1>

        {user ? (
          <div className="auth-signed-in">
            <p>You are signed in as <strong>{user.email}</strong>.</p>
            <a className="button button--dark" href="/basket">Continue to your bag</a>
            <button className="text-button" onClick={logout} type="button">Sign out</button>
          </div>
        ) : (
          <form className="auth-form" onSubmit={(event) => void handleSubmit(event)}>
            {mode === "register" && (
              <>
                <label>
                  Display name
                  <input
                    autoComplete="name"
                    onChange={(event) => setDisplayName(event.target.value)}
                    required
                    value={displayName}
                  />
                </label>
                <label>
                  Username
                  <input
                    autoComplete="username"
                    onChange={(event) => setUsername(event.target.value)}
                    required
                    value={username}
                  />
                </label>
                <label>
                  Phone number
                  <input
                    autoComplete="tel"
                    onChange={(event) => setPhoneNumber(event.target.value)}
                    required
                    value={phoneNumber}
                  />
                </label>
              </>
            )}
            <label>
              Email
              <input
                autoComplete="email"
                onChange={(event) => setEmail(event.target.value)}
                required
                type="email"
                value={email}
              />
            </label>
            <label>
              Password
              <input
                autoComplete={mode === "login" ? "current-password" : "new-password"}
                onChange={(event) => setPassword(event.target.value)}
                required
                type="password"
                value={password}
              />
            </label>
            {error && <p className="inline-error" role="alert">{error}</p>}
            <button className="button button--dark" disabled={submitting} type="submit">
              {submitting
                ? "Please wait..."
                : mode === "login"
                  ? "Sign in"
                  : "Create account"}
            </button>
            <p className="auth-switch">
              {mode === "login" ? "New to Forma?" : "Already have an account?"}{" "}
              <button
                className="text-button"
                onClick={() => {
                  setMode(mode === "login" ? "register" : "login");
                  setError(null);
                }}
                type="button"
              >
                {mode === "login" ? "Create an account" : "Sign in"}
              </button>
            </p>
          </form>
        )}
      </div>
    </section>
  );
}
