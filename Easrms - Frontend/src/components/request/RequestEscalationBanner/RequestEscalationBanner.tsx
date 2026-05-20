import { Alert } from "@mui/material";

interface RequestEscalationBannerProps {
  escalatedOn: string | null;
  escalatedByName: string | null;
  escalationReason: string | null;
}

const RequestEscalationBanner = ({
  escalatedOn,
  escalatedByName,
  escalationReason,
}: RequestEscalationBannerProps) => {
  return (
    <Alert severity="warning" sx={{ mb: 2 }}>
      <strong>Escalated by {escalatedByName} on {escalatedOn}:</strong> {escalationReason}
    </Alert>
  );
};

export default RequestEscalationBanner;
