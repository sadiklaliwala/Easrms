import { Button, Stack } from "@mui/material";
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
    <Stack
      direction={mode === "link" ? "row" : "column"}
      spacing={mode === "login" ? 1.5 : 0}
      sx={{
        flexWrap: mode === "link" ? "wrap" : "nowrap",
        gap: mode === "link" ? 1.5 : 0,
      }}
    >
      {OAUTH_PROVIDERS.map((provider) => {
        const linked = mode === "link" && isLinked(provider);

        return (
          <Button
            key={provider}
            variant="outlined"
            fullWidth={mode === "login"}
            disabled={linked}
            startIcon={getAuthProviderIcon(provider)}
            endIcon={
              linked ? (
                <CheckCircleIcon sx={{ color: "success.main", fontSize: 18 }} />
              ) : undefined
            }
            onClick={() => handleClick(provider)}
            sx={{
              py: mode === "login" ? 1.25 : 1,
              px: mode === "login" ? 0 : 2.5,
              borderRadius: mode === "link" ? 10 : 2, // Pill shape for link mode
              textTransform: "none",
              fontWeight: 600,
              fontSize: "0.875rem",
              borderColor: linked ? "success.light" : "divider",
              color: "text.primary",
              justifyContent: mode === "login" ? "center" : "flex-start",
              bgcolor: linked ? "success.50" : "background.paper",
              boxShadow:
                mode === "link" ? "0 2px 6px rgba(0,0,0,0.03)" : "none",
              transition: "all 0.2s ease",
              "&:hover": {
                borderColor: "primary.main",
                bgcolor: "rgba(79, 70, 229, 0.04)",
                transform: mode === "link" ? "translateY(-1px)" : "none",
                boxShadow:
                  mode === "link" ? "0 4px 12px rgba(0,0,0,0.08)" : "none",
              },
            }}
          >
            {mode === "login"
              ? `Continue with ${PROVIDER_LABELS[provider]}`
              : linked
                ? `${PROVIDER_LABELS[provider]} Linked`
                : `Link ${PROVIDER_LABELS[provider]}`}
          </Button>
        );
      })}
    </Stack>
  );
};

export default OAuthButtons;
