export interface ProfileDetailDto {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  managerName?: string;
  profilePhotoUrl?: string;
  createdOn: string;
  lastLoginOn?: string;
}

export interface UpdateProfileDto {
  fullName: string;
  profilePhotoUrl?: string;
}

export interface ChangePasswordDto {
  passwordChangeToken: string;
  newPassword: string;
  confirmPassword: string;
}

export interface VerifyProfileOtpDto {
  otpCode: string;
}

export interface VerifyProfileOtpResponseDto {
  passwordChangeToken: string;
}

export interface SendOtpDto {
  email: string;
}

export interface VerifyOtpDto {
  email: string;
  otpCode: string;
}

export interface VerifyOtpResponseDto {
  passwordResetToken: string;
}

export interface ResetPasswordDto {
  email: string;
  passwordResetToken: string;
  newPassword: string;
  confirmPassword: string;
}
