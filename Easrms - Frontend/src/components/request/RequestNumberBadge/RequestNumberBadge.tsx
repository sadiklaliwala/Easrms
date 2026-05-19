import { Chip } from "@mui/material";

interface RequestNumberBadgeProps {
  requestNumber: string;
}

const RequestNumberBadge = ({ requestNumber }: RequestNumberBadgeProps) => {
  return (
    <Chip
      label={requestNumber}
      size="small"
      variant="outlined"
      color="primary"
      sx={{ fontWeight: 600, fontFamily: "monospace" }}
    />
  );
};

export default RequestNumberBadge;