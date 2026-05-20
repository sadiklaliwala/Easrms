import { Box, Divider, Typography } from "@mui/material";
import { formatDate } from "../../../utils/formatDate";
import AppSLABadge from "../../common/table/AppSLABadge";

interface RequestSLAInfoProps {
  dueDate: string | null;
  slaStatus: string;
  isEscalated: boolean;
}

const MetaRow = ({ label, value }: { label: string; value?: React.ReactNode }) => {
  if (value === undefined || value === null) return null;
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        py: 1,
      }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }} component="div">
        {value}
      </Typography>
    </Box>
  );
};

const RequestSLAInfo = ({
  dueDate,
  slaStatus,
  isEscalated,
}: RequestSLAInfoProps) => {
  return (
    <Box>
      <MetaRow label="Due Date" value={dueDate ? formatDate(dueDate) : "N/A"} />
      <Divider />
      <MetaRow label="SLA Status" value={<AppSLABadge slaStatus={slaStatus} />} />
      <Divider />
      <MetaRow 
        label="Escalated" 
        value={
          <Typography variant="body2" color={isEscalated ? "error.main" : "text.secondary"} sx={{ fontWeight: 600 }}>
            {isEscalated ? "Yes" : "No"}
          </Typography>
        } 
      />
    </Box>
  );
};

export default RequestSLAInfo;
