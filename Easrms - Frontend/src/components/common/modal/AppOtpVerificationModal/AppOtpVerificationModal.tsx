import React, { useState, useRef, useEffect } from "react";
import {
  Dialog,
  Box,
  Typography,
  Button,
  Stack,
  keyframes,
} from "@mui/material";
import AppLoadingButton from "../../buttons/AppLoadingButton";

// ─── Animations ──────────────────────────────────────────────────────────────
const shake = keyframes`
  0% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  50% { transform: translateX(5px); }
  75% { transform: translateX(-5px); }
  100% { transform: translateX(0); }
`;

interface AppOtpVerificationModalProps {
  open: boolean;
  onClose: () => void;
  email: string;
  onVerify: (otp: string) => Promise<boolean>;
  onResend: () => Promise<void>;
  timer: number;
}

const AppOtpVerificationModal = ({
  open,
  onClose,
  email,
  onVerify,
  onResend,
  timer,
}: AppOtpVerificationModalProps) => {
  const [otp, setOtp] = useState<string[]>(new Array(6).fill(""));
  const [status, setStatus] = useState<"idle" | "error" | "success">("idle");
  const [isVerifying, setIsVerifying] = useState(false);
  const [isResending, setIsResending] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Focus first input on open
  useEffect(() => {
    if (open) {
      setOtp(new Array(6).fill(""));
      setStatus("idle");
      setTimeout(() => inputRefs.current[0]?.focus(), 100);
    }
  }, [open]);

  const handleChange = (
    index: number,
    e: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const value = e.target.value;
    if (isNaN(Number(value))) return;

    const newOtp = [...otp];
    // Take the last character typed
    newOtp[index] = value.substring(value.length - 1);
    setOtp(newOtp);
    setStatus("idle");

    // Move to next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (
    index: number,
    e: React.KeyboardEvent<HTMLInputElement>,
  ) => {
    if (e.key === "Backspace") {
      const newOtp = [...otp];
      if (!otp[index] && index > 0) {
        // Move to previous if current is empty
        newOtp[index - 1] = "";
        setOtp(newOtp);
        inputRefs.current[index - 1]?.focus();
      } else {
        newOtp[index] = "";
        setOtp(newOtp);
      }
      setStatus("idle");
    }
  };

  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    e.preventDefault();
    const pastedData = e.clipboardData
      .getData("text/plain")
      .slice(0, 6)
      .split("");
    const newOtp = [...otp];
    pastedData.forEach((char, index) => {
      if (!isNaN(Number(char)) && index < 6) {
        newOtp[index] = char;
      }
    });
    setOtp(newOtp);
    setStatus("idle");
    const nextEmptyIndex = newOtp.findIndex((val) => val === "");
    if (nextEmptyIndex !== -1) {
      inputRefs.current[nextEmptyIndex]?.focus();
    } else {
      inputRefs.current[5]?.focus();
    }
  };

  const handleVerify = async () => {
    const otpCode = otp.join("");
    if (otpCode.length !== 6) {
      setStatus("error");
      return;
    }

    setIsVerifying(true);
    setStatus("idle");
    const success = await onVerify(otpCode);
    if (success) {
      setStatus("success");
      // Add a slight delay before closing so user sees the green success state
      setTimeout(() => {
        onClose();
        setOtp(new Array(6).fill(""));
        setStatus("idle");
      }, 1000);
    } else {
      setStatus("error");
    }
    setIsVerifying(false);
  };

  const handleResend = async () => {
    setIsResending(true);
    await onResend();
    setIsResending(false);
    setOtp(new Array(6).fill(""));
    setStatus("idle");
    inputRefs.current[0]?.focus();
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      slotProps={{
        backdrop: {
          sx: {
            backdropFilter: "blur(8px)",
            backgroundColor: "rgba(0,0,0,0.4)",
          },
        },
        paper: {
          sx: {
            borderRadius: 4,
            p: 4,
            width: "100%",
            maxWidth: 420,
            background: "background.paper",
            color: "text.primary",
            border: "1px solid",
            borderColor: "divider",
            boxShadow: "0 8px 32px rgba(0,0,0,0.1)",
          },
        },
      }}
    >
      <Box sx={{ textAlign: "center", mb: 4 }}>
        <Typography variant="h5" sx={{ fontWeight: 800, mb: 1.5 }}>
          Email Verification
        </Typography>
        <Typography variant="body2" sx={{ color: "text.secondary" }}>
          We sent a 6-digit code to <strong>{email}</strong>. Enter it below to
          continue.
        </Typography>
      </Box>

      <Box sx={{ textAlign: "left", mb: 4 }}>
        <Typography
          variant="caption"
          sx={{
            fontWeight: 600,
            color: "text.secondary",
            mb: 1,
            display: "block",
          }}
        >
          6-digit code
        </Typography>
        <Stack
          sx={{
            flexDirection: "row",
            gap: 1.5,
            justifyContent: "center",
            animation: status === "error" ? `${shake} 0.4s ease` : "none",
          }}
        >
          {otp.map((digit, index) => (
            <Box
              component="input"
              key={index}
              ref={(el: any) => (inputRefs.current[index] = el)}
              type="text"
              inputMode="numeric"
              maxLength={1}
              value={digit}
              onChange={(e: any) => handleChange(index, e)}
              onKeyDown={(e: any) => handleKeyDown(index, e)}
              onPaste={handlePaste}
              sx={{
                width: "48px",
                height: "56px",
                borderRadius: "12px",
                background: "transparent",
                border: "2px solid",
                borderColor:
                  status === "error"
                    ? "error.main" // red
                    : status === "success"
                      ? "success.main" // green
                      : digit
                        ? "primary.main" // active/filled
                        : "divider", // idle
                color: "text.primary",
                fontSize: "1.5rem",
                fontWeight: "700",
                textAlign: "center",
                outline: "none",
                transition: "all 0.2s ease",
                boxShadow:
                  status === "error"
                    ? "0 0 12px rgba(239, 68, 68, 0.2)"
                    : status === "success"
                      ? "0 0 12px rgba(34, 197, 94, 0.2)"
                      : digit
                        ? "0 0 12px rgba(99, 102, 241, 0.1)"
                        : "none",
                "&:focus": {
                  borderColor: status === "idle" ? "primary.main" : undefined,
                  boxShadow:
                    status === "idle"
                      ? "0 0 12px rgba(99, 102, 241, 0.1)"
                      : undefined,
                },
              }}
            />
          ))}
        </Stack>
      </Box>

      <Stack spacing={2}>
        <AppLoadingButton
          label="Verify"
          onClick={handleVerify}
          loading={isVerifying}
          fullWidth
          size="large"
          sx={{
            py: 1.5,
            borderRadius: 3,
            fontWeight: 700,
            textTransform: "none",
            fontSize: "1rem",
          }}
        />

        <Button
          variant="outlined"
          onClick={handleResend}
          disabled={timer > 0 || isResending}
          fullWidth
          size="large"
          sx={{
            py: 1.5,
            borderRadius: 3,
            textTransform: "none",
            fontWeight: 600,
          }}
        >
          {timer > 0 ? `Resend code in ${timer}s` : "Resend code"}
        </Button>
      </Stack>
    </Dialog>
  );
};

export default AppOtpVerificationModal;
