import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";
import type {
  ProfileDetailDto,
  UpdateProfileDto,
  VerifyProfileOtpDto,
  VerifyProfileOtpResponseDto,
  ChangePasswordDto,
} from "../../types/profile.types";

const profileEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getProfile: builder.query<ApiResponse<ProfileDetailDto>, void>({
      query: () => ApiEndPoints.PROFILE,
    }),
    updateProfile: builder.mutation<
      ApiResponse<ProfileDetailDto>,
      UpdateProfileDto
    >({
      query: (body) => ({
        url: ApiEndPoints.PROFILE,
        method: "PUT",
        body,
      }),
    }),
    sendProfileOtp: builder.mutation<ApiResponse<null>, void>({
      query: () => ({
        url: ApiEndPoints.PROFILE_SEND_OTP,
        method: "POST",
      }),
    }),
    verifyProfileOtp: builder.mutation<
      ApiResponse<VerifyProfileOtpResponseDto>,
      VerifyProfileOtpDto
    >({
      query: (body) => ({
        url: ApiEndPoints.PROFILE_VERIFY_OTP,
        method: "POST",
        body,
      }),
    }),
    changePassword: builder.mutation<ApiResponse<null>, ChangePasswordDto>({
      query: (body) => ({
        url: ApiEndPoints.PROFILE_CHANGE_PASSWORD,
        method: "PUT",
        body,
      }),
    }),
  }),
});

export const {
  useGetProfileQuery,
  useUpdateProfileMutation,
  useSendProfileOtpMutation,
  useVerifyProfileOtpMutation,
  useChangePasswordMutation,
} = profileEndpoints;
