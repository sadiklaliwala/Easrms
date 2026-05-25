import { useCallback } from "react";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "joi";
import { Box, Paper, Typography, Stack, Avatar } from "@mui/material";
import toast from "react-hot-toast";
import PersonIcon from "@mui/icons-material/Person";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { useGetLinkedProvidersQuery } from "../../../store/api/auth.endpoints";
import { AuthProvider } from "../../../types/common.types";
import LinkedAccountsSection from "../../../components/common/profile/LinkedAccountsSection";
import AppPasswordInput from "../../../components/common/form/AppPasswordInput";
import AppLoadingButton from "../../../components/common/buttons/AppLoadingButton";
import AppFormError from "../../../components/common/form/AppFormError";
import AppLabel from "../../../components/common/form/AppLabel";
import AppSkeletonLoader from "../../../components/common/feedback/AppSkeletonLoader";

// ─── Change Password Validation ───────────────────────────────────────────────
const changePasswordSchema = Joi.object({
  currentPassword: Joi.string().min(6).required().messages({
    "string.empty": "Current password is required",
    "string.min": "Password must be at least 6 characters",
  }),
  newPassword: Joi.string().min(6).required().messages({
    "string.empty": "New password is required",
    "string.min": "Password must be at least 6 characters",
  }),
  confirmPassword: Joi.string()
    .valid(Joi.ref("newPassword"))
    .required()
    .messages({
      "string.empty": "Please confirm your password",
      "any.only": "Passwords do not match",
    }),
});

interface ChangePasswordForm {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// ─── Component ────────────────────────────────────────────────────────────────
const ProfilePage = () => {
  const { fullName, email, roleName } = useAppSelector((state) => state.auth);
  const { data: linkedResponse, isLoading: isLoadingProviders } =
    useGetLinkedProvidersQuery();

  const linkedProviders = linkedResponse?.data ?? [];
  const isLocalLinked = linkedProviders.some(
    (p) => p.authProvider === AuthProvider.Local,
  );

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ChangePasswordForm>({
    resolver: joiResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  const onChangePassword = useCallback(
    async (_data: ChangePasswordForm) => {
      // TODO: Integrate with backend change-password endpoint
      toast.success("Password changed successfully!");
      reset();
    },
    [reset],
  );

  return (
    <Box>
      {/* Page Header */}
      <Typography
        variant="h5"
        sx={{ fontWeight: 800, color: "primary.main", mb: 3 }}
      >
        My Profile
      </Typography>

      <Stack spacing={3}>
        {/* ── Personal Info Section ── */}
        <Paper
          elevation={0}
          sx={{
            p: 3,
            borderRadius: 3,
            border: "1px solid",
            borderColor: "divider",
          }}
        >
          <Typography
            variant="subtitle1"
            sx={{ fontWeight: 700, mb: 2, color: "text.primary" }}
          >
            Personal Information
          </Typography>

          <Stack
            component="div"
            spacing={3}
            sx={{
              py: 1,
              flexDirection: {
                xs: "column",
                sm: "row",
              },
              alignItems: {
                xs: "center",
                sm: "flex-start",
              },
              textAlign: {
                xs: "center",
                sm: "left",
              },
              justifyContent: "center",
            }}
          >
            <Avatar
              sx={{
                width: 72,
                height: 72,
                bgcolor: "rgba(79, 70, 229, 0.08)",
                color: "secondary.main",
                border: "2px solid",
                borderColor: "secondary.light",
              }}
            >
              <PersonIcon sx={{ fontSize: 36 }} />
            </Avatar>

            <Stack
              spacing={0.75}
              sx={{
                alignItems: {
                  xs: "center",
                  sm: "flex-start",
                },
              }}
            >
              <Typography
                variant="h6"
                sx={{ fontWeight: 700, color: "text.primary" }}
              >
                {fullName ?? "—"}
              </Typography>
              <Typography
                variant="body2"
                sx={{ color: "text.secondary", fontWeight: 500 }}
              >
                {email ?? "—"}
              </Typography>
              <Typography
                variant="caption"
                sx={{
                  color: "secondary.main",
                  fontWeight: 600,
                  bgcolor: "rgba(79, 70, 229, 0.08)",
                  px: 1.5,
                  py: 0.5,
                  borderRadius: 1,
                  display: "inline-block",
                  width: "fit-content",
                }}
              >
                {roleName ?? "—"}
              </Typography>
            </Stack>
          </Stack>
        </Paper>

        {/* ── Change Password Section (only if Local provider linked) ── */}
        {isLoadingProviders ? (
          <Paper
            elevation={0}
            sx={{
              p: 3,
              borderRadius: 3,
              border: "1px solid",
              borderColor: "divider",
            }}
          >
            <AppSkeletonLoader rows={3} />
          </Paper>
        ) : (
          isLocalLinked && (
            <Paper
              elevation={0}
              sx={{
                p: 3,
                borderRadius: 3,
                border: "1px solid",
                borderColor: "divider",
              }}
            >
              <Typography
                variant="subtitle1"
                sx={{ fontWeight: 700, mb: 2, color: "text.primary" }}
              >
                Change Password
              </Typography>

              <Stack
                component="form"
                onSubmit={handleSubmit(onChangePassword)}
                spacing={2.5}
                sx={{ maxWidth: 400 }}
              >
                <Box>
                  <AppLabel label="Current Password" required />
                  <Controller
                    name="currentPassword"
                    control={control}
                    render={({ field }) => (
                      <AppPasswordInput
                        {...field}
                        placeholder="Enter current password"
                        fullWidth
                        error={!!errors.currentPassword}
                        autoComplete="current-password"
                      />
                    )}
                  />
                  <AppFormError message={errors.currentPassword?.message} />
                </Box>

                <Box>
                  <AppLabel label="New Password" required />
                  <Controller
                    name="newPassword"
                    control={control}
                    render={({ field }) => (
                      <AppPasswordInput
                        {...field}
                        placeholder="Enter new password"
                        fullWidth
                        error={!!errors.newPassword}
                        autoComplete="new-password"
                      />
                    )}
                  />
                  <AppFormError message={errors.newPassword?.message} />
                </Box>

                <Box>
                  <AppLabel label="Confirm Password" required />
                  <Controller
                    name="confirmPassword"
                    control={control}
                    render={({ field }) => (
                      <AppPasswordInput
                        {...field}
                        placeholder="Confirm new password"
                        fullWidth
                        error={!!errors.confirmPassword}
                        autoComplete="new-password"
                      />
                    )}
                  />
                  <AppFormError message={errors.confirmPassword?.message} />
                </Box>

                <AppLoadingButton
                  label="Update Password"
                  loading={false}
                  type="submit"
                  size="large"
                  sx={{ py: 1.25, borderRadius: 2, alignSelf: "flex-start" }}
                />
              </Stack>
            </Paper>
          )
        )}

        {/* ── Linked Accounts Section ── */}
        <Paper
          elevation={0}
          sx={{
            p: 3,
            borderRadius: 3,
            border: "1px solid",
            borderColor: "divider",
          }}
        >
          <LinkedAccountsSection />
        </Paper>
      </Stack>
    </Box>
  );
};

export default ProfilePage;
