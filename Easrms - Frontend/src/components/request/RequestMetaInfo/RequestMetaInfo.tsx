import { Box, Divider, Typography } from "@mui/material";
import { formatDate } from "../../../utils/formatDate";

interface RequestMetaInfoProps {
  employeeName: string;
  assigneeName?: string;
  createdOn: string;
  updatedOn?: string;
  resolvedOn?: string;
  closedOn?: string;
}

const MetaRow = ({ label, value }: { label: string; value?: string }) => {
  if (!value) return null;
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        py: 1,
      }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }}>
        {value}
      </Typography>
    </Box>
  );
};

const RequestMetaInfo = ({
  employeeName,
  assigneeName,
  createdOn,
  updatedOn,
  resolvedOn,
  closedOn,
}: RequestMetaInfoProps) => {
  return (
    <Box>
      <MetaRow label="Raised By" value={employeeName} />
      <Divider />
      <MetaRow label="Assigned To" value={assigneeName} />
      <Divider />
      <MetaRow label="Created On" value={formatDate(createdOn)} />
      <Divider />
      <MetaRow label="Last Updated On" value={formatDate(updatedOn)} />
      <Divider />
      <MetaRow label="Resolved On" value={formatDate(resolvedOn)} />
      <Divider />
      <MetaRow label="Closed On" value={formatDate(closedOn)} />
    </Box>
  );
};

export default RequestMetaInfo;
