import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";

export interface CloudinarySignatureResponse {
  apiKey: string;
  cloudName: string;
  timestamp: string;
  signature: string;
  folder: string;
}

export interface UploadSignatureRequest {
  folder?: string;
}

const cloudinaryEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getCloudinarySignature: builder.mutation<
      ApiResponse<CloudinarySignatureResponse>,
      UploadSignatureRequest
    >({
      query: (body) => ({
        url: ApiEndPoints.CLOUDINARY.SIGN,
        method: "POST",
        body,
      }),
    }),
  }),
});

export const { useGetCloudinarySignatureMutation } = cloudinaryEndpoints;
