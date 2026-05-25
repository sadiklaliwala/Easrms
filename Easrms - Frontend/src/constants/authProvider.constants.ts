import { AuthProvider } from "../types/common.types";

export const PROVIDER_LABELS: Record<number, string> = {
  [AuthProvider.Local]: "Local",
  [AuthProvider.Google]: "Google",
  [AuthProvider.GitHub]: "GitHub",
  [AuthProvider.Azure]: "Azure",
};

export const PROVIDER_COLORS: Record<
  number,
  "default" | "primary" | "secondary" | "success" | "error" | "info" | "warning"
> = {
  [AuthProvider.Local]: "default",
  [AuthProvider.Google]: "error",
  [AuthProvider.GitHub]: "default",
  [AuthProvider.Azure]: "primary",
};

export const OAUTH_URLS = {
  Google: "https://accounts.google.com/o/oauth2/v2/auth",
  GitHub: "https://github.com/login/oauth/authorize",
  Azure: "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize",
};
