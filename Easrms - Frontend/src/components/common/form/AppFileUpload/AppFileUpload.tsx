import React, { useState } from "react";
import { Button, Box, Typography, CircularProgress } from "@mui/material";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { useGetCloudinarySignatureMutation } from "../../../../store/api/cloudinary.endpoints";

interface AppFileUploadProps {
  label?: string;
  onUploadSuccess: (url: string) => void;
  onUploadStart?: () => void;
  onError?: (error: string) => void;
  value?: string | null;
}

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

const AppFileUpload: React.FC<AppFileUploadProps> = ({
  label = "Upload Attachment",
  onUploadSuccess,
  onUploadStart,
  onError,
  value,
}) => {
  const [getSignature] = useGetCloudinarySignatureMutation();
  const [isUploading, setIsUploading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (file.size > MAX_FILE_SIZE) {
      const msg = "File size exceeds the 10MB limit.";
      setErrorMsg(msg);
      onError?.(msg);
      return;
    }

    setErrorMsg(null);
    setIsUploading(true);
    onUploadStart?.();

    try {
      // 1. Get Signature
      const sigResponse = await getSignature({ folder: "Easrms_Upload" }).unwrap();
      const { apiKey, cloudName, timestamp, signature, folder } = sigResponse.data;

      // 2. Upload to Cloudinary
      const formData = new FormData();
      formData.append("file", file);
      formData.append("api_key", apiKey);
      formData.append("timestamp", timestamp);
      formData.append("signature", signature);
      formData.append("folder", folder);

      const uploadResponse = await fetch(
        `https://api.cloudinary.com/v1_1/${cloudName}/auto/upload`,
        {
          method: "POST",
          body: formData,
        }
      );

      const uploadResult = await uploadResponse.json();

      if (!uploadResponse.ok) {
        throw new Error(uploadResult.error?.message || "Failed to upload file");
      }

      onUploadSuccess(uploadResult.secure_url);
    } catch (err: any) {
      const msg = err.message || "An error occurred during file upload.";
      setErrorMsg(msg);
      onError?.(msg);
    } finally {
      setIsUploading(false);
      // Reset the input so the same file can be selected again if needed
      event.target.value = '';
    }
  };

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5 }}>
      {label && <Typography variant="caption" color="text.secondary">{label} (Max 10MB)</Typography>}
      <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
        <Button
          component="label"
          variant="outlined"
          color={errorMsg ? "error" : "primary"}
          startIcon={isUploading ? <CircularProgress size={20} /> : <CloudUploadIcon />}
          disabled={isUploading}
          sx={{ textTransform: 'none' }}
        >
          {isUploading ? "Uploading..." : value ? "Replace File" : "Choose File"}
          <input
            type="file"
            hidden
            onChange={handleFileChange}
          />
        </Button>
        {value && !isUploading && (
          <Typography variant="body2" color="success.main" sx={{
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
              maxWidth: "200px"
          }}>
            Attached
          </Typography>
        )}
      </Box>
      {errorMsg && <Typography variant="caption" color="error">{errorMsg}</Typography>}
    </Box>
  );
};

export default AppFileUpload;
