import { Button, Stack, Typography } from "@mui/material";
import { AuthProvider } from "../../../../types/common.types";
import { PROVIDER_LABELS } from "../../../../constants/authProvider.constants";
import { getAuthProviderIcon } from "../../../../utils/getAuthProviderIcon";
import {
  getGoogleOAuthUrl,
  getGitHubOAuthUrl,
  getAzureOAuthUrl,
  getLinkedInOAuthUrl,
} from "../../../../utils/getOAuthUrl";
import type { LinkedProviderDto } from "../../../../types/auth.types";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";

interface OAuthButtonsProps {
  mode: "login" | "link";
  linkedProviders?: LinkedProviderDto[];
  onSuccess?: () => void;
}

const OAUTH_PROVIDERS = [
  AuthProvider.Google,
  AuthProvider.GitHub,
  AuthProvider.Azure,
  AuthProvider.LinkedIn,
];

const getOAuthUrl = (provider: AuthProvider): string => {
  switch (provider) {
    case AuthProvider.Google:
      return getGoogleOAuthUrl();
    case AuthProvider.GitHub:
      return getGitHubOAuthUrl();
    case AuthProvider.Azure:
      return getAzureOAuthUrl();
    case AuthProvider.LinkedIn:
      return getLinkedInOAuthUrl();
    default:
      return "";
  }
};

const OAuthButtons = ({ mode, linkedProviders = [] }: OAuthButtonsProps) => {
  const handleClick = (provider: AuthProvider) => {
    // Store flow info in sessionStorage before redirecting
    sessionStorage.setItem(
      "oauth_flow",
      JSON.stringify({ flow: mode, provider }),
    );

    const url = getOAuthUrl(provider);
    if (url) {
      window.location.href = url;
    }
  };

  const isLinked = (provider: AuthProvider) =>
    linkedProviders.some((p) => p.authProvider === provider);

  return (
    <Stack spacing={1.5}>
      {OAUTH_PROVIDERS.map((provider) => {
        const linked = mode === "link" && isLinked(provider);

        return (
          <Button
            key={provider}
            variant="outlined"
            fullWidth
            disabled={linked}
            startIcon={getAuthProviderIcon(provider)}
            endIcon={
              linked ? (
                <CheckCircleIcon sx={{ color: "success.main" }} />
              ) : undefined
            }
            onClick={() => handleClick(provider)}
            sx={{
              py: 1.25,
              borderRadius: 2,
              textTransform: "none",
              fontWeight: 600,
              fontSize: "0.875rem",
              borderColor: "divider",
              color: "text.primary",
              justifyContent: "center",
              "&:hover": {
                borderColor: "primary.main",
                bgcolor: "rgba(79, 70, 229, 0.04)",
              },
            }}
          >
            {mode === "login"
              ? `Continue with ${PROVIDER_LABELS[provider]}`
              : linked
                ? `${PROVIDER_LABELS[provider]} — Linked`
                : `Link ${PROVIDER_LABELS[provider]}`}
            {linked && (
              <Typography
                component="span"
                variant="caption"
                sx={{ ml: 1, color: "success.main", fontWeight: 600 }}
              >
                ✓
              </Typography>
            )}
          </Button>
        );
      })}
    </Stack>
  );
};

export default OAuthButtons;
