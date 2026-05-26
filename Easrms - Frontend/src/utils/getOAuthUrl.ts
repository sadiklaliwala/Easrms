import { OAUTH_URLS } from "../constants/authProvider.constants";

const redirectUri = import.meta.env.VITE_REDIRECT_URI;

export const generateOAuthState = () => {
  const state = Math.random().toString(36).substring(2, 15);
  sessionStorage.setItem("oauth_state", state);
  return state;
};

export const getGoogleOAuthUrl = () => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  const state = generateOAuthState();
  const params = new URLSearchParams({
    client_id: clientId,
    redirect_uri: redirectUri,
    response_type: "code",
    scope: "openid email profile",
    state: state,
    prompt: "select_account",
  });
  return `${OAUTH_URLS.Google}?${params.toString()}`;
};

export const getGitHubOAuthUrl = () => {
  const clientId = import.meta.env.VITE_GITHUB_CLIENT_ID;
  const state = generateOAuthState();
  const params = new URLSearchParams({
    client_id: clientId,
    redirect_uri: redirectUri,
    scope: "user:email",
    state: state,
  });
  return `${OAUTH_URLS.GitHub}?${params.toString()}`;
};

export const getAzureOAuthUrl = () => {
  const clientId = import.meta.env.VITE_AZURE_CLIENT_ID;
  const tenantId = import.meta.env.VITE_AZURE_TENANT_ID;
  const state = generateOAuthState();
  const params = new URLSearchParams({
    client_id: clientId,
    redirect_uri: redirectUri,
    response_type: "code",
    scope: "openid email profile User.Read",
    state: state,
    response_mode: "query",
  });
  const baseUrl = OAUTH_URLS.Azure.replace("{tenant}", tenantId);
  return `${baseUrl}?${params.toString()}`;
};

export const getLinkedInOAuthUrl = () => {
  const clientId = import.meta.env.VITE_LINKEDIN_CLIENT_ID;
  const state = generateOAuthState();
  const params = new URLSearchParams({
    response_type: "code",
    client_id: clientId,
    redirect_uri: redirectUri,
    state: state,
    scope: "openid email profile",
  });
  return `${OAUTH_URLS.LinkedIn}?${params.toString()}`;
};
