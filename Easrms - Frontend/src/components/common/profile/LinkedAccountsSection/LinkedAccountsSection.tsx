import {
  Box,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Typography,
  Tooltip,
  Chip,
} from "@mui/material";
import LinkOffIcon from "@mui/icons-material/LinkOff";
import toast from "react-hot-toast";

import {
  useGetLinkedProvidersQuery,
  useUnlinkProviderMutation,
} from "../../../../store/api/auth.endpoints";
import {
  PROVIDER_LABELS,
  PROVIDER_COLORS,
} from "../../../../constants/authProvider.constants";
import { getAuthProviderIcon } from "../../../../utils/getAuthProviderIcon";
import { AuthProvider } from "../../../../types/common.types";
import AppSkeletonLoader from "../../feedback/AppSkeletonLoader";
import AppButton from "../../buttons/AppButton";
import AppLoadingButton from "../../buttons/AppLoadingButton";
import OAuthButtons from "../../buttons/OAuthButtons";

const LinkedAccountsSection = () => {
  const { data: response, isLoading } = useGetLinkedProvidersQuery();
  const [unlinkProvider, { isLoading: isUnlinking }] =
    useUnlinkProviderMutation();

  const linkedProviders = response?.data ?? [];

  const handleUnlink = async (providerId: string, providerName: string) => {
    try {
      const result = await unlinkProvider({ providerId }).unwrap();
      if (result.success) {
        toast.success(`${providerName} unlinked successfully`);
      } else {
        toast.error(result.message ?? "Failed to unlink provider");
      }
    } catch (err: any) {
      const message =
        err?.data?.message ??
        err?.data?.errors?.[0] ??
        "Failed to unlink provider";
      toast.error(message);
    }
  };

  if (isLoading) {
    return <AppSkeletonLoader rows={3} />;
  }

  const canUnlink = linkedProviders.length > 1;

  return (
    <Box>
      <Typography
        variant="subtitle1"
        sx={{ fontWeight: 700, mb: 2, color: "text.primary" }}
      >
        Linked Accounts
      </Typography>

      {/* List of currently linked providers */}
      <List disablePadding>
        {linkedProviders.map((provider) => (
          <ListItem
            key={provider.id}
            sx={{
              px: 2,
              py: 1.5,
              mb: 1,
              borderRadius: 2,
              border: "1px solid",
              borderColor: "divider",
              bgcolor: "background.paper",
            }}
            secondaryAction={
              provider.authProvider ===
              AuthProvider.Local ? null : canUnlink ? (
                <AppLoadingButton
                  label="Unlink"
                  loading={isUnlinking}
                  variant="outlined"
                  color="error"
                  size="small"
                  startIcon={<LinkOffIcon />}
                  onClick={() =>
                    handleUnlink(provider.id, provider.providerName)
                  }
                  sx={{ textTransform: "none" }}
                />
              ) : (
                <Tooltip title="Cannot unlink only remaining provider">
                  <span>
                    <AppButton
                      label="Unlink"
                      variant="outlined"
                      color="error"
                      size="small"
                      disabled
                      startIcon={<LinkOffIcon />}
                      sx={{ textTransform: "none" }}
                    />
                  </span>
                </Tooltip>
              )
            }
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              {getAuthProviderIcon(provider.authProvider)}
            </ListItemIcon>
            <ListItemText
              primary={
                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    {provider.providerName}
                  </Typography>
                  <Chip
                    label={PROVIDER_LABELS[provider.authProvider]}
                    size="small"
                    color={PROVIDER_COLORS[provider.authProvider]}
                    variant="outlined"
                    sx={{ height: 22, fontSize: "0.7rem" }}
                  />
                </Box>
              }
              secondary={`Linked on ${new Date(provider.linkedOn).toLocaleDateString()}`}
            />
          </ListItem>
        ))}
      </List>

      {/* Link new providers */}
      <Box sx={{ mt: 3 }}>
        <Typography
          variant="subtitle2"
          sx={{ fontWeight: 600, mb: 1.5, color: "text.secondary" }}
        >
          Link Additional Provider
        </Typography>
        <OAuthButtons mode="link" linkedProviders={linkedProviders} />
      </Box>
    </Box>
  );
};

export default LinkedAccountsSection;
