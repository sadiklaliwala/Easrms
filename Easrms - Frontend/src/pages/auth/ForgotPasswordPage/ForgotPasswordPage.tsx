import { useState, useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "../../../utils/appJoi";
import { Box, Paper, Typography, Stack, Button } from "@mui/material";
import toast from "react-hot-toast";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import {
  useResetPasswordMutation,
  useSendOtpMutation,
  useVerifyOtpMutation,
} from "../../../store/api/auth.endpoints";
import AppLabel from "../../../components/common/form/AppLabel";
import AppOtpVerificationModal from "../../../components/common/modal/AppOtpVerificationModal";
import AppInput from "../../../components/common/form/AppInput";
import AppFormError from "../../../components/common/form/AppFormError";
import AppLoadingButton from "../../../components/common/buttons/AppLoadingButton";
import AppPasswordInput from "../../../components/common/form/AppPasswordInput";

// ─── Step 1 (Email Input) Joi Schema ──────────────────────────────────────────
const step1Schema = Joi.object({
  email: Joi.string()
    .email({ tlds: { allow: false } })
    .max(140)
    .required()
    .messages({
      "string.empty": "Email address is required",
      "string.email": "Please enter a valid email address",
      "string.max": "Email must not exceed 140 characters",
    }),
});

interface Step1Form {
  email: string;
}

// ─── Step 3 (Reset Password) Joi Schema ───────────────────────────────────────
const step3Schema = Joi.object({
  newPassword: Joi.string().min(8).max(255).required().messages({
    "string.empty": "New password is required",
    "string.min": "Password must be at least 8 characters",
    "string.max": "Password must not exceed 255 characters",
  }),
  confirmPassword: Joi.string()
    .valid(Joi.ref("newPassword"))
    .max(255)
    .required()
    .messages({
      "string.empty": "Please confirm your password",
      "any.only": "Passwords do not match",
      "string.max": "Password must not exceed 255 characters",
    }),
});

interface Step3Form {
  newPassword: string;
  confirmPassword: string;
}

const ForgotPasswordPage = () => {
  const navigate = useNavigate();

  const [sendOtp, { isLoading: isSendingOtp }] = useSendOtpMutation();
  const [verifyOtp] = useVerifyOtpMutation();
  const [resetPassword, { isLoading: isResettingPassword }] =
    useResetPasswordMutation();

  // Local States
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [email, setEmail] = useState<string>("");

  const [passwordResetToken, setPasswordResetToken] = useState<string>("");
  const [timer, setTimer] = useState<number>(0);

  // Countdown timer for Resend OTP button
  useEffect(() => {
    if (timer > 0) {
      const interval = setInterval(() => {
        setTimer((prev) => prev - 1);
      }, 1000);
      return () => clearInterval(interval);
    }
  }, [timer]);

  // Step 1 Form
  const {
    control: emailControl,
    handleSubmit: handleEmailSubmit,
    formState: { errors: emailErrors },
  } = useForm<Step1Form>({
    resolver: joiResolver(step1Schema),
    defaultValues: { email: "" },
  });

  // Step 3 Form
  const {
    control: passwordControl,
    handleSubmit: handlePasswordSubmit,
    formState: { errors: passwordErrors },
  } = useForm<Step3Form>({
    resolver: joiResolver(step3Schema),
    defaultValues: { newPassword: "", confirmPassword: "" },
  });

  // Action - Send OTP
  const onSendEmailOtp = async (data: Step1Form) => {
    try {
      const result = await sendOtp({ email: data.email }).unwrap();
      if (result.success) {
        toast.success("OTP sent if email is registered");
        setEmail(data.email);
        setTimer(60);
        setStep(2);
      } else {
        toast.error(result.message || "Failed to send OTP");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to send OTP");
    }
  };

  // Action - Resend OTP (using state email)
  const onResendOtp = async () => {
    try {
      const result = await sendOtp({ email }).unwrap();
      if (result.success) {
        toast.success("OTP sent successfully");
        setTimer(60);
      } else {
        toast.error(result.message || "Failed to resend OTP");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to resend OTP");
    }
  };

  // Action - Verify OTP
  const onVerifyOtpModal = async (otpCode: string): Promise<boolean> => {
    try {
      const result = await verifyOtp({ email, otpCode }).unwrap();
      if (result.success && result.data) {
        setPasswordResetToken(result.data.passwordResetToken);
        toast.success("OTP verified successfully");
        setStep(3);
        return true;
      } else {
        toast.error(result.message || "Invalid OTP code");
        return false;
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to verify OTP");
      return false;
    }
  };

  // Action - Reset Password
  const onSetNewPassword = async (data: Step3Form) => {
    try {
      const result = await resetPassword({
        email,
        passwordResetToken,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      }).unwrap();

      if (result.success) {
        toast.success("Password reset successfully. Please login.");
        navigate("/login", { replace: true });
      } else {
        toast.error(result.message || "Failed to reset password");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to reset password");
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
        elevation={0}
        sx={{
          p: 4,
          width: "100%",
          maxWidth: 450,
          borderRadius: 3,
          border: "1px solid",
          borderColor: "divider",
          boxShadow: "0px 8px 24px rgba(0, 0, 0, 0.05)",
        }}
      >
        {(step === 1 || step === 2) && (
          <Stack
            component="form"
            onSubmit={handleEmailSubmit(onSendEmailOtp)}
            spacing={3}
          >
            <Box>
              <Typography
                variant="h5"
                sx={{ fontWeight: 800, mb: 1, color: "text.primary" }}
              >
                Forgot Password
              </Typography>
              <Typography variant="body2" sx={{ color: "text.secondary" }}>
                Enter your registered email address and we'll send you an OTP to
                reset your password.
              </Typography>
            </Box>

            <Box>
              <AppLabel label="Email Address" required />
              <Controller
                name="email"
                control={emailControl}
                render={({ field }) => (
                  <AppInput
                    {...field}
                    placeholder="Enter email address"
                    error={!!emailErrors.email}
                    autoComplete="email"
                    maxLength={140}
                  />
                )}
              />
              <AppFormError message={emailErrors.email?.message} />
            </Box>

            <AppLoadingButton
              label="Send OTP"
              loading={isSendingOtp}
              type="submit"
              fullWidth
              size="large"
              sx={{ py: 1.25, borderRadius: 2 }}
            />

            <Button
              variant="text"
              startIcon={<ArrowBackIcon />}
              onClick={() => navigate("/login")}
              sx={{
                textTransform: "none",
                fontWeight: 600,
                color: "text.secondary",
              }}
            >
              Back to Login
            </Button>
          </Stack>
        )}

        <AppOtpVerificationModal
          open={step === 2}
          onClose={() => {
            setStep(1);
          }}
          email={email}
          onVerify={onVerifyOtpModal}
          onResend={onResendOtp}
          timer={timer}
        />

        {step === 3 && (
          <Stack
            component="form"
            onSubmit={handlePasswordSubmit(onSetNewPassword)}
            spacing={3}
          >
            <Box>
              <Typography
                variant="h5"
                sx={{ fontWeight: 800, mb: 1, color: "text.primary" }}
              >
                Set New Password
              </Typography>
              <Typography variant="body2" sx={{ color: "text.secondary" }}>
                Create a strong password of at least 8 characters.
              </Typography>
            </Box>

            <Box>
              <AppLabel label="New Password" required />
              <Controller
                name="newPassword"
                control={passwordControl}
                render={({ field }) => (
                  <AppPasswordInput
                    {...field}
                    placeholder="Enter new password"
                    error={!!passwordErrors.newPassword}
                    maxLength={255}
                  />
                )}
              />
              <AppFormError message={passwordErrors.newPassword?.message} />
            </Box>

            <Box>
              <AppLabel label="Confirm Password" required />
              <Controller
                name="confirmPassword"
                control={passwordControl}
                render={({ field }) => (
                  <AppPasswordInput
                    {...field}
                    placeholder="Confirm new password"
                    error={!!passwordErrors.confirmPassword}
                    maxLength={255}
                  />
                )}
              />
              <AppFormError message={passwordErrors.confirmPassword?.message} />
            </Box>

            <AppLoadingButton
              label="Reset Password"
              loading={isResettingPassword}
              type="submit"
              fullWidth
              size="large"
              sx={{ py: 1.25, borderRadius: 2 }}
            />
          </Stack>
        )}
      </Paper>
    </Box>
  );
};

export default ForgotPasswordPage;
