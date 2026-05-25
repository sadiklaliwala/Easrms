import { useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "joi";
import { Box, Paper, Typography, Stack, Divider } from "@mui/material";
import toast from "react-hot-toast";

import { useLoginMutation } from "../../../store/api/auth.endpoints";
import { setCredentials } from "../../../store/slices/authSlice";
import { useAppDispatch } from "../../../hooks/useAppSelector";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { ROLES } from "../../../constants/role.constants";

import AppInput from "../../../components/common/form/AppInput";
import AppPasswordInput from "../../../components/common/form/AppPasswordInput";
import AppLoadingButton from "../../../components/common/buttons/AppLoadingButton";
import AppFormError from "../../../components/common/form/AppFormError";
import AppLabel from "../../../components/common/form/AppLabel";
import OAuthButtons from "../../../components/common/buttons/OAuthButtons";

import BusinessCenterIcon from "@mui/icons-material/BusinessCenter";

import type { LoginRequestDto } from "../../../types/auth.types";

// ─── Validation Schema ────────────────────────────────────────────────────────
const schema = Joi.object({
  email: Joi.string()
    .email({ tlds: { allow: false } })
    .required()
    .messages({
      "string.empty": "Email is required",
      "string.email": "Enter a valid email address",
    }),
  password: Joi.string().min(1).required().messages({
    "string.empty": "Password is required",
  }),
});

// ─── Component ────────────────────────────────────────────────────────────────
const LoginPage = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { isAuthenticated, roleName } = useAppSelector((state) => state.auth);

  const [login, { isLoading }] = useLoginMutation();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequestDto>({
    resolver: joiResolver(schema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  // useCallback — stable reference, won't recreate on every render
  const redirectByRole = useCallback(
    (role: string) => {
      if (role === ROLES.ADMIN || role === ROLES.MANAGER) {
        navigate("/dashboard", { replace: true });
      } else if (role === ROLES.EMPLOYEE) {
        navigate("/dashboard", { replace: true });
      } else if (role === ROLES.SUPPORT_USER) {
        navigate("/my-tasks", { replace: true });
      }
    },
    [navigate],
  );

  // If already logged in, redirect to correct landing
  useEffect(() => {
    if (isAuthenticated && roleName) {
      redirectByRole(roleName);
    }
  }, [isAuthenticated, roleName, redirectByRole]);

  // useCallback — stable reference for form submit handler
  const onSubmit = useCallback(
    async (data: LoginRequestDto) => {
      try {
        const response = await login(data).unwrap();

        if (response.success && response.data) {
          dispatch(setCredentials(response.data));
          toast.success(`Welcome back, ${response.data.fullName}!`);
          redirectByRole(response.data.roleName);
        } else {
          toast.error(response.message ?? "Login failed");
        }
      } catch (err: any) {
        const message =
          err?.data?.message ??
          err?.data?.errors?.[0] ??
          "Invalid email or password";
        toast.error(message);
      }
    },
    [login, dispatch, redirectByRole],
  );

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background:
          "radial-gradient(circle at 10% 20%, rgba(99, 102, 241, 0.05) 0%, rgba(255, 255, 255, 0) 40%), radial-gradient(circle at 90% 80%, rgba(79, 70, 229, 0.06) 0%, rgba(255, 255, 255, 0) 50%), #f8fafc",
        px: 2,
        position: "relative",
        overflow: "hidden",
      }}
    >
      {/* Background radial blurs */}
      <Box
        sx={{
          position: "absolute",
          top: -100,
          left: -100,
          width: 300,
          height: 300,
          borderRadius: "50%",
          bgcolor: "rgba(99, 102, 241, 0.08)",
          filter: "blur(80px)",
          pointerEvents: "none",
        }}
      />
      <Box
        sx={{
          position: "absolute",
          bottom: -150,
          right: -150,
          width: 400,
          height: 400,
          borderRadius: "50%",
          bgcolor: "rgba(79, 70, 229, 0.08)",
          filter: "blur(100px)",
          pointerEvents: "none",
        }}
      />

      <Paper
        elevation={0}
        sx={{
          width: "100%",
          maxWidth: 420,
          p: { xs: 4, sm: 5 },
          borderRadius: 3,
          border: "1px solid",
          borderColor: "divider",
          boxShadow:
            "0 10px 25px -5px rgba(0, 0, 0, 0.03), 0 8px 10px -6px rgba(0, 0, 0, 0.03)",
          position: "relative",
          zIndex: 1,
          bgcolor: "background.paper",
        }}
      >
        {/* Brand Icon */}
        <Box
          sx={{
            display: "flex",
            justifyContent: "center",
            mb: 2.5,
          }}
        >
          <Box
            sx={{
              width: 52,
              height: 52,
              borderRadius: "14px",
              bgcolor: "rgba(79, 70, 229, 0.08)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "secondary.main",
            }}
          >
            <BusinessCenterIcon sx={{ fontSize: 28 }} />
          </Box>
        </Box>

        {/* Header */}
        <Stack
          spacing={0.5}
          sx={{ mb: 4, alignItems: "center", textAlign: "center" }}
        >
          <Typography
            variant="h5"
            sx={{
              fontWeight: 800,
              color: "primary.main",
              letterSpacing: "-0.02em",
            }}
          >
            EASRMS
          </Typography>
          <Typography
            variant="body2"
            sx={{ color: "text.secondary", fontWeight: 500 }}
          >
            Employee Asset & Service Request Management
          </Typography>
        </Stack>

        {/* Form */}
        <Stack
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          spacing={3}
          noValidate
        >
          {/* Email */}
          <Box>
            <AppLabel label="Email" required />
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <AppInput
                  {...field}
                  placeholder="Enter your email"
                  type="email"
                  fullWidth
                  error={!!errors.email}
                  autoComplete="email"
                  autoFocus
                />
              )}
            />
            <AppFormError message={errors.email?.message} />
          </Box>

          {/* Password */}
          <Box>
            <AppLabel label="Password" required />
            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <AppPasswordInput
                  {...field}
                  placeholder="Enter your password"
                  fullWidth
                  error={!!errors.password}
                  autoComplete="current-password"
                />
              )}
            />
            <AppFormError message={errors.password?.message} />
          </Box>

          {/* Submit */}
          <AppLoadingButton
            label="Sign In"
            loading={isLoading}
            type="submit"
            fullWidth
            size="large"
            sx={{ py: 1.25, borderRadius: 2 }}
          />
        </Stack>

        {/* OAuth Divider */}
        <Divider sx={{ my: 3, color: "text.secondary", fontSize: "0.8rem" }}>
          OR
        </Divider>

        {/* OAuth Buttons */}
        <OAuthButtons mode="login" />
      </Paper>
    </Box>
  );
};

export default LoginPage;
