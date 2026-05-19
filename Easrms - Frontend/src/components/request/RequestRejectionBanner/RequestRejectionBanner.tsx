import { Alert } from "@mui/material";

interface RequestRejectionBannerProps {
  reason: string;
}

const RequestRejectionBanner = ({ reason }: RequestRejectionBannerProps) => {
  return (
    <Alert severity="error" sx={{ mb: 2 }}>
      <strong>Rejected:</strong> {reason}
    </Alert>
  );
};

export default RequestRejectionBanner;