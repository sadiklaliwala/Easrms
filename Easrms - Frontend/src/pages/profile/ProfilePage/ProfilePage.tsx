import { useState, useRef, useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { joiResolver } from "@hookform/resolvers/joi";
import Joi from "../../../utils/appJoi";
import {
  Box,
  Paper,
  Typography,
  Stack,
  Avatar,
  IconButton,
  Grid,
  Chip,
} from "@mui/material";
import toast from "react-hot-toast";
import CameraAltIcon from "@mui/icons-material/CameraAlt";
import EmailIcon from "@mui/icons-material/Email";
import BadgeIcon from "@mui/icons-material/Badge";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";

import AppLoadingButton from "../../../components/common/buttons/AppLoadingButton";
import AppSkeletonLoader from "../../../components/common/feedback/AppSkeletonLoader";
import AppOtpVerificationModal from "../../../components/common/modal/AppOtpVerificationModal";
import AppFormError from "../../../components/common/form/AppFormError";
import AppInput from "../../../components/common/form/AppInput";
import AppLabel from "../../../components/common/form/AppLabel";
import AppPasswordInput from "../../../components/common/form/AppPasswordInput";
import LinkedAccountsSection from "../../../components/common/profile/LinkedAccountsSection";
import {
  useGetProfileQuery,
  useUpdateProfileMutation,
  useSendProfileOtpMutation,
  useVerifyProfileOtpMutation,
  useChangePasswordMutation,
} from "../../../store/api/profile.endpoints";
import ApiEndPoints from "../../../store/ApiEndPoints";

// ─── Change Password Step 3 Schema ───────────────────────────────────────────
const changePasswordSchema = Joi.object({
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

interface ChangePasswordForm {
  newPassword: string;
  confirmPassword: string;
}

const ProfilePage = () => {
  const {
    data: profileResponse,
    isLoading: isLoadingProfile,
    refetch,
  } = useGetProfileQuery();
  const [updateProfile, { isLoading: isUpdatingProfile }] =
    useUpdateProfileMutation();
  const [sendProfileOtp, { isLoading: isSendingOtp }] =
    useSendProfileOtpMutation();
  const [verifyProfileOtp] = useVerifyProfileOtpMutation();
  const [changePassword, { isLoading: isChangingPassword }] =
    useChangePasswordMutation();

  const profile = profileResponse?.data;

  // Local States
  const [profilePhotoUrl, setProfilePhotoUrl] = useState<string>("");
  const [isUploadingPhoto, setIsUploadingPhoto] = useState<boolean>(false);
  const [step, setStep] = useState<1 | 2 | 3>(1);

  const [passwordChangeToken, setPasswordChangeToken] = useState<string>("");
  const [timer, setTimer] = useState<number>(0);

  const fileInputRef = useRef<HTMLInputElement>(null);

  // Sync profile photo from API response
  useEffect(() => {
    if (profile?.profilePhotoUrl) {
      setProfilePhotoUrl(profile.profilePhotoUrl);
    }
  }, [profile]);

  // Resend OTP countdown timer
  useEffect(() => {
    if (timer > 0) {
      const interval = setInterval(() => {
        setTimer((prev) => prev - 1);
      }, 1000);
      return () => clearInterval(interval);
    }
  }, [timer]);

  // Profile Form (React Hook Form)
  const {
    control: profileControl,
    handleSubmit: handleProfileSubmit,
    reset: resetProfileForm,
    formState: { errors: profileErrors },
  } = useForm<{ fullName: string }>({
    defaultValues: { fullName: "" },
  });

  // Reset form once profile data is loaded
  useEffect(() => {
    if (profile) {
      resetProfileForm({ fullName: profile.fullName });
    }
  }, [profile, resetProfileForm]);

  // Change Password Form (React Hook Form)
  const {
    control: passwordControl,
    handleSubmit: handlePasswordSubmit,
    reset: resetPasswordForm,
    formState: { errors: passwordErrors },
  } = useForm<ChangePasswordForm>({
    resolver: joiResolver(changePasswordSchema),
    defaultValues: { newPassword: "", confirmPassword: "" },
  });

  // Photo upload handler
  const handlePhotoUpload = async (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setIsUploadingPhoto(true);
    try {
      // 1. Get Signature via plain fetch call
      const signUrl = `${import.meta.env.VITE_API_BASE_URL || ""}${ApiEndPoints.CLOUDINARY.SIGN}`;
      const signResponse = await fetch(signUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ folder: "profile-photos" }),
        credentials: "include",
      });

      if (!signResponse.ok) {
        throw new Error("Failed to sign Cloudinary request");
      }

      const sigData = await signResponse.json();
      const { apiKey, cloudName, timestamp, signature, folder } = sigData.data;

      // 2. Build FormData
      const formData = new FormData();
      formData.append("file", file);
      formData.append("api_key", apiKey);
      formData.append("timestamp", timestamp);
      formData.append("signature", signature);
      formData.append("folder", folder);

      // 3. Upload to Cloudinary
      const uploadResponse = await fetch(
        `https://api.cloudinary.com/v1_1/${cloudName}/image/upload`,
        {
          method: "POST",
          body: formData,
        },
      );

      const uploadResult = await uploadResponse.json();

      if (!uploadResponse.ok) {
        throw new Error(
          uploadResult.error?.message || "Failed to upload image to Cloudinary",
        );
      }

      // Update state for preview
      setProfilePhotoUrl(uploadResult.secure_url);
      toast.success(
        "Image uploaded successfully! Click save to update your profile.",
      );
    } catch (err: any) {
      toast.error(err.message || "Failed to upload photo");
    } finally {
      setIsUploadingPhoto(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  // Profile Save
  const onSaveProfile = async (formData: { fullName: string }) => {
    try {
      const result = await updateProfile({
        fullName: formData.fullName,
        profilePhotoUrl: profilePhotoUrl || undefined,
      }).unwrap();

      if (result.success) {
        toast.success("Profile updated successfully");
        refetch();
      } else {
        toast.error(result.message || "Failed to update profile");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to update profile");
    }
  };

  // Change Password - Send OTP
  const onSendOtp = async () => {
    try {
      const result = await sendProfileOtp().unwrap();
      if (result.success) {
        toast.success("OTP sent to your registered email");
        setTimer(60);
        setStep(2);
      } else {
        toast.error(result.message || "Failed to send OTP");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to send OTP");
    }
  };

  // Change Password - Verify OTP
  const onVerifyOtpModal = async (otpCode: string): Promise<boolean> => {
    try {
      const result = await verifyProfileOtp({ otpCode }).unwrap();
      if (result.success && result.data) {
        setPasswordChangeToken(result.data.passwordChangeToken);
        toast.success("OTP verified successfully");
        setStep(3);
        return true;
      } else {
        toast.error(result.message || "Failed to verify OTP");
        return false;
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to verify OTP");
      return false;
    }
  };

  // Change Password - Reset Password
  const onChangePassword = async (data: ChangePasswordForm) => {
    try {
      const result = await changePassword({
        passwordChangeToken,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      }).unwrap();

      if (result.success) {
        toast.success("Password changed successfully!");
        setStep(1);
        setPasswordChangeToken("");
        resetPasswordForm();
      } else {
        toast.error(result.message || "Failed to change password");
      }
    } catch (err: any) {
      toast.error(err?.data?.message || "Failed to change password");
    }
  };

  const getInitials = (nameStr?: string) => {
    if (!nameStr) return "";
    return nameStr
      .split(" ")
      .map((part) => part[0])
      .join("")
      .toUpperCase()
      .substring(0, 2);
  };

  if (isLoadingProfile) {
    return (
      <Box sx={{ p: 3 }}>
        <AppSkeletonLoader rows={5} />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 1 }}>
      {/* Page Header */}
      <Typography
        variant="h5"
        sx={{ fontWeight: 800, color: "primary.main", mb: 3 }}
      >
        My Profile
      </Typography>

      <Grid container spacing={3}>
        {/* Left Column - Profile Info Card */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Paper
            elevation={0}
            sx={{
              p: 3,
              borderRadius: 3,
              border: "1px solid",
              borderColor: "divider",
              textAlign: "center",
            }}
          >
            <Typography
              variant="subtitle1"
              sx={{
                fontWeight: 700,
                mb: 3,
                color: "text.primary",
                textAlign: "left",
              }}
            >
              Personal Information
            </Typography>

            {/* Avatar Upload Container */}
            <Box
              sx={{
                position: "relative",
                display: "inline-block",
                mx: "auto",
                mb: 3,
              }}
            >
              <Avatar
                src={profilePhotoUrl || undefined}
                sx={{
                  width: 110,
                  height: 110,
                  fontSize: "2rem",
                  fontWeight: 600,
                  bgcolor: "primary.light",
                  color: "primary.contrastText",
                  border: "3px solid",
                  borderColor: "divider",
                  boxShadow: "0px 4px 10px rgba(0, 0, 0, 0.08)",
                }}
              >
                {getInitials(profile?.fullName)}
              </Avatar>

              {/* Upload Button overlay */}
              <IconButton
                component="label"
                disabled={isUploadingPhoto}
                sx={{
                  position: "absolute",
                  bottom: -2,
                  right: -2,
                  bgcolor: "background.paper",
                  border: "1px solid",
                  borderColor: "divider",
                  boxShadow: "0px 2px 5px rgba(0, 0, 0, 0.1)",
                  width: 32,
                  height: 32,
                  "&:hover": {
                    bgcolor: "grey.100",
                  },
                }}
              >
                <CameraAltIcon sx={{ fontSize: 16, color: "text.secondary" }} />
                <input
                  type="file"
                  ref={fileInputRef}
                  hidden
                  accept="image/*"
                  onChange={handlePhotoUpload}
                />
              </IconButton>
            </Box>

            {/* Editable Form */}
            <Stack
              component="form"
              onSubmit={handleProfileSubmit(onSaveProfile)}
              spacing={2.5}
              sx={{ textAlign: "left" }}
            >
              <Box>
                <AppLabel label="Full Name" required />
                <Controller
                  name="fullName"
                  control={profileControl}
                  rules={{
                    required: "Full name is required",
                    maxLength: {
                      value: 90,
                      message: "Full name must not exceed 90 characters",
                    },
                  }}
                  render={({ field }) => (
                    <AppInput
                      {...field}
                      placeholder="Enter full name"
                      error={!!profileErrors.fullName}
                      maxLength={90}
                    />
                  )}
                />
                <AppFormError message={profileErrors.fullName?.message} />
              </Box>

              <Box>
                <AppLabel label="Email Address" />
                <AppInput
                  value={profile?.email || ""}
                  disabled
                  slotProps={{
                    input: {
                      startAdornment: (
                        <EmailIcon
                          sx={{ color: "text.secondary", mr: 1, fontSize: 18 }}
                        />
                      ),
                    },
                  }}
                />
              </Box>

              <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                <Typography
                  variant="body2"
                  sx={{ fontWeight: 600, color: "text.secondary" }}
                >
                  Role:
                </Typography>
                <Chip
                  label={profile?.roleName || "—"}
                  color="primary"
                  variant="outlined"
                  size="small"
                  icon={<BadgeIcon sx={{ fontSize: 14 }} />}
                  sx={{ fontWeight: 600 }}
                />
              </Box>

              {profile?.managerName && (
                <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                  <Typography
                    variant="body2"
                    sx={{ fontWeight: 600, color: "text.secondary" }}
                  >
                    Manager:
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ color: "text.primary", fontWeight: 500 }}
                  >
                    {profile.managerName}
                  </Typography>
                </Box>
              )}

              <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                <CalendarTodayIcon
                  sx={{ color: "text.secondary", fontSize: 16 }}
                />
                <Typography
                  variant="caption"
                  sx={{ color: "text.secondary", fontWeight: 500 }}
                >
                  Member since{" "}
                  {profile?.createdOn
                    ? new Date(profile.createdOn).toLocaleDateString(
                        undefined,
                        { year: "numeric", month: "long", day: "numeric" },
                      )
                    : "—"}
                </Typography>
              </Box>

              <AppLoadingButton
                label="Save Changes"
                loading={isUpdatingProfile || isUploadingPhoto}
                type="submit"
                variant="contained"
                color="primary"
                fullWidth
                sx={{ mt: 1, borderRadius: 2 }}
              />
            </Stack>
          </Paper>
        </Grid>

        {/* Right Column - Change Password & Linked Accounts */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Stack spacing={3}>
            {/* Section 2 - Change Password Card */}
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

              {(step === 1 || step === 2) && (
                <Stack spacing={2}>
                  <Typography variant="body2" sx={{ color: "text.secondary" }}>
                    To change your password, we will send a 6-digit One Time
                    Password (OTP) to your registered email address for
                    verification.
                  </Typography>
                  <AppLoadingButton
                    label="Send OTP"
                    loading={isSendingOtp}
                    onClick={onSendOtp}
                    sx={{ alignSelf: "flex-start", px: 4, borderRadius: 2 }}
                  />
                </Stack>
              )}

              <AppOtpVerificationModal
                open={step === 2}
                onClose={() => {
                  setStep(1);
                }}
                email={profile?.email || ""}
                onVerify={onVerifyOtpModal}
                onResend={onSendOtp}
                timer={timer}
              />

              {step === 3 && (
                <Stack
                  component="form"
                  onSubmit={handlePasswordSubmit(onChangePassword)}
                  spacing={2.5}
                >
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
                    <AppFormError
                      message={passwordErrors.newPassword?.message}
                    />
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
                    <AppFormError
                      message={passwordErrors.confirmPassword?.message}
                    />
                  </Box>

                  <AppLoadingButton
                    label="Change Password"
                    loading={isChangingPassword}
                    type="submit"
                    sx={{ alignSelf: "flex-start", px: 4, borderRadius: 2 }}
                  />
                </Stack>
              )}
            </Paper>

            {/* Section 3 - Linked Accounts Section */}
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
        </Grid>
      </Grid>
    </Box>
  );
};

export default ProfilePage;
