import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "joi";
import { Box, Paper, Typography, Stack } from "@mui/material";
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

  // If already logged in, redirect to correct landing
  useEffect(() => {
    if (isAuthenticated && roleName) {
      redirectByRole(roleName);
    }
  }, [isAuthenticated, roleName]);

  const redirectByRole = (role: string) => {
    if (role === ROLES.ADMIN || role === ROLES.MANAGER) {
      navigate("/dashboard", { replace: true });
    } else if (role === ROLES.EMPLOYEE) {
      navigate("/requests", { replace: true });
    } else if (role === ROLES.SUPPORT_USER) {
      navigate("/my-tasks", { replace: true });
    }
  };

  const onSubmit = async (data: LoginRequestDto) => {
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
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        bgcolor: "background.default",
        px: 2,
      }}
    >
      <Paper
        elevation={3}
        sx={{
          width: "100%",
          maxWidth: 440,
          p: { xs: 3, sm: 4 },
          borderRadius: 2,
        }}
      >
        {/* Header */}
        <Stack spacing={0.5} mb={4} alignItems="center">
          <Typography variant="h5" fontWeight={700} color="primary">
            EASRMS
          </Typography>
          <Typography variant="body2" color="text.secondary">
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
          />
        </Stack>
      </Paper>
    </Box>
  );
};

export default LoginPage;
